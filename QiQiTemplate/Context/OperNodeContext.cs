﻿using QiQiTemplate.Enums;
using QiQiTemplate.Model;
using QiQiTemplate.Provide;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace QiQiTemplate.Context
{
    /// <summary>
    /// 运算
    /// </summary>
    public class OperNodeContext : NodeContext
    {
        private static readonly Regex SetRegex = new Regex(@"^\s*{{#oper\s(?<name>[a-zA-Z_][\w]*)(?<oper>([+][+]|--))}}\s*$", RegexOptions.Compiled);
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="code"></param>
        /// <param name="parent"></param>
        /// <param name="output"></param>
        public OperNodeContext(string code, NodeBlockContext parent, OutPutProvide output)
            : base(code, parent, output)
        {
            NdType = NodeType.OPER;
        }
        /// <summary>
        /// 节点信息
        /// </summary>
        public SetModel Model { get; private set; } = default!;
        /// <summary>
        /// 转换表达式
        /// </summary>
        public override void ConvertToExpression()
        {
            var name = ParentNode.SearchVariable(Model.Name);
            if (Model.Oper == "++")
            {
                NdExpression = Expression.PostIncrementAssign(name);
            }
            else
            {
                NdExpression = Expression.PostDecrementAssign(name);
            }
        }
        /// <summary>
        /// 解析语句
        /// </summary>
        protected override void ParsingModel()
        {
            var gp = SetRegex.Match(CodeString);
            Model = new SetModel
            {
                Name = gp.Groups["name"].Value,
                Oper = gp.Groups["oper"].Value,
            };
        }
    }
}