﻿using QiQiTemplate.Context;
using QiQiTemplate.Enum;
using QiQiTemplate.Model;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace QiQiTemplate.Provide
{
    /// <summary>
    /// 几点解析提供类
    /// </summary>
    public class NodeContextProvide
    {
        /// <summary>
        /// 行号
        /// </summary>
        private int _lineNumber = 0;
        /// <summary>
        /// 针对if的匹配
        /// </summary>
        protected static readonly Regex IsIFRegex = new Regex(@"^\s*{{#if\s+.+}}\s*$", RegexOptions.Compiled);
        /// <summary>
        /// 针对if结束的匹配
        /// </summary>
        protected static readonly Regex IsENDIFRegex = new Regex(@"^\s*{{#/if}}\s*$", RegexOptions.Compiled);
        /// <summary>
        /// 针对else if的匹配
        /// </summary>
        protected static readonly Regex IsELSEIFRegex = new Regex(@"^\s*{{#else\s+if\s+.+}}\s*$", RegexOptions.Compiled);
        /// <summary>
        /// else if 结束
        /// </summary>
        protected static readonly Regex IsENDELSEIFRegex = new Regex(@"^\s*{{#/else if}}\s*$");
        /// <summary>
        /// 针对else的匹配
        /// </summary>
        protected static readonly Regex IsELSERegex = new Regex(@"^\s*{{#else}}\s*$", RegexOptions.Compiled);
        /// <summary>
        /// else 结束
        /// </summary>
        protected static readonly Regex IsENDELSERegex = new Regex(@"^\s*{{#/else}}\s*$");
        /// <summary>
        /// 针对each循环的匹配
        /// </summary>
        protected static readonly Regex IsEACHRegex = new Regex(@"^\s*{{#each\s+((?!{{|}}).)+}}\s*$", RegexOptions.Compiled);
        /// <summary>
        /// 针对each结束的匹配
        /// </summary>
        protected static readonly Regex IsENDEACHRegex = new Regex(@"^\s*{{#/each}}\s*$", RegexOptions.Compiled);
        /// <summary>
        /// 针对print的匹配
        /// </summary>
        protected static readonly Regex IsPRINTRegex = new Regex("({{[^{](((?!{{|}}).)+)}})+", RegexOptions.Compiled);
        /// <summary>
        /// 针对define的匹配
        /// </summary>
        protected static readonly Regex IsDEFINERegex = new Regex(@"^\s*{{#define\s[a-zA-Z_][\w]+.+}}\s*$", RegexOptions.Compiled);
        /// <summary>
        /// 针对set的匹配
        /// </summary>
        protected static readonly Regex IsSETRegex = new Regex(@"^{{#set\s[a-zA-Z_][\w]*([+][+]|--)}}$");

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public Expression<Action<DynamicModel>> BuildTemplateByReader(StreamReader reader, OutPutProvide output)
        {
            var scope = new ScopeBlockContext();
            using var _reader = reader;
            CreateNodeContextRange(_reader, scope, output);
            var node = scope.Nodes;
            scope.ConvertToExpression();
            return Expression.Lambda<Action<DynamicModel>>(scope.NdExpression, scope.Root);
        }

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="path"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public Expression<Action<DynamicModel>> BuildTemplateByPath(string path, OutPutProvide output)
        {
            var reader = new StreamReader(path);
            return BuildTemplateByReader(reader, output);
        }

        /// <summary>
        /// 编译模板
        /// </summary>
        /// <param name="template"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public Expression<Action<DynamicModel>> BuildTemplateByString(string template, OutPutProvide output)
        {
            using var memory = new MemoryStream();
            using var writer = new StreamWriter(memory);
            writer.Write(template);
            memory.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(memory);
            return BuildTemplateByReader(reader, output);
        }

        /// <summary>
        /// 将语法解析为节点树
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="ParentNode"></param>
        /// <param name="output"></param>
        private void CreateNodeContextRange(StreamReader reader, NodeBlockContext ParentNode, OutPutProvide output)
        {
            while (true)
            {
                string line = reader.ReadLine();
                if (line == null) return;
                _lineNumber++;
                NodeType type = this.GetNodeType(line);
                NodeContext last = ParentNode.Nodes.LastOrDefault();
                if (last?.NdType == NodeType.IF || last?.NdType == NodeType.ELSEIF)
                {
                    if (type != NodeType.ELSEIF && type != NodeType.ELSE)
                    {
                        for (int i = ParentNode.Nodes.Count - 1; i >= 0; i--)
                        {
                            NodeContext item = ParentNode.Nodes[i];
                            if (item.NdType != NodeType.ELSEIF && item.NdType != NodeType.IF)
                                break;
                            item.ConvertToExpression();
                        }
                    }
                }
                switch (type)
                {
                    case NodeType.IF:
                        NodeBlockContext block = new IFNodeContext(line, ParentNode, output);
                        CreateNodeContextRange(reader, block, output);
                        ParentNode.Nodes.Add(block);
                        break;
                    case NodeType.ELSEIF:
                        block = new ELSEIFNodeContext(line, ParentNode, output);
                        CreateNodeContextRange(reader, block, output);
                        if (last is IFNodeContext ifnd1)
                        {
                            ifnd1.ELSENode = block;
                        }
                        else if (last is ELSEIFNodeContext elnd1)
                        {
                            elnd1.ELSENode = block;
                        }
                        else
                        {
                            throw new Exception($"第{_lineNumber}行语法错误,else if必须在 if 或 else if 之后");
                        }
                        ParentNode.Nodes.Add(block);
                        break;
                    case NodeType.ELSE:
                        block = new ELSENodeContext(line, ParentNode, output);
                        CreateNodeContextRange(reader, block, output);
                        block.ConvertToExpression();
                        if (last is IFNodeContext ifnd)
                        {
                            ifnd.ELSENode = block;
                        }
                        else if (last is ELSEIFNodeContext elnd)
                        {
                            elnd.ELSENode = block;
                        }
                        else
                        {
                            throw new Exception($"第{_lineNumber}行语法错误,else 必须在 if 或 else if之后");
                        }

                        for (int i = ParentNode.Nodes.Count - 1; i >= 0; i--)
                        {
                            NodeContext item = ParentNode.Nodes[i];
                            if (item.NdType != NodeType.ELSEIF && item.NdType != NodeType.IF)
                                break;
                            item.ConvertToExpression();
                        }

                        ParentNode.Nodes.Add(block);
                        break;
                    case NodeType.EACH:
                        block = new EACHNodeContext(line, ParentNode, output);
                        CreateNodeContextRange(reader, block, output);
                        block.ConvertToExpression();
                        ParentNode.Nodes.Add(block);
                        break;
                    case NodeType.PRINT:
                        NodeContext node = new PRINTNodeContext(line, ParentNode, output);
                        node.ConvertToExpression();
                        ParentNode.Nodes.Add(node);
                        break;
                    case NodeType.STRING:
                        node = new STRINGNodeContext(line, ParentNode, output);
                        node.ConvertToExpression();
                        ParentNode.Nodes.Add(node);
                        break;
                    case NodeType.DEFINE:
                        node = new DEFINENodeContext(line, ParentNode, output);
                        node.ConvertToExpression();
                        ParentNode.Nodes.Add(node);
                        break;
                    case NodeType.SET:
                        node = new SETNodeContext(line, ParentNode, output);
                        node.ConvertToExpression();
                        ParentNode.Nodes.Add(node);
                        break;
                    case NodeType.ENDIF:
                    case NodeType.ENDELSEIF:
                    case NodeType.ENDELSE:
                    case NodeType.ENDEACH:
                        return;
                    default:
                        throw new Exception($"{type}是不受支持的节点类型");
                }
            }
        }

        /// <summary>
        /// 获取代码节点的类型
        /// [需要注意顺序]
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected NodeType GetNodeType(string code)
        {
            return code switch
            {
                string msg when IsIFRegex.IsMatch(msg) => NodeType.IF,
                string msg when IsENDIFRegex.IsMatch(msg) => NodeType.ENDIF,
                string msg when IsELSEIFRegex.IsMatch(msg) => NodeType.ELSEIF,
                string msg when IsENDELSEIFRegex.IsMatch(msg) => NodeType.ENDELSEIF,
                string msg when IsELSERegex.IsMatch(msg) => NodeType.ELSE,
                string msg when IsENDELSERegex.IsMatch(msg) => NodeType.ENDELSE,
                string msg when IsEACHRegex.IsMatch(msg) => NodeType.EACH,
                string msg when IsENDEACHRegex.IsMatch(msg) => NodeType.ENDEACH,
                string msg when IsDEFINERegex.IsMatch(msg) => NodeType.DEFINE,
                string msg when IsSETRegex.IsMatch(msg) => NodeType.SET,
                string msg when IsPRINTRegex.IsMatch(msg) => NodeType.PRINT,
                _ => NodeType.STRING,
            };
        }

    }
}
