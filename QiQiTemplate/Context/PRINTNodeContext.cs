﻿using QiQiTemplate.Enums;
using QiQiTemplate.Model;
using QiQiTemplate.Provide;
using QiQiTemplate.Tool;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace QiQiTemplate.Context
{
    /// <summary>
    /// 输出节点
    /// </summary>
    public class PRINTNodeContext : NodeContext
    {
        private static readonly Regex ParsingReges2 = new Regex(@"{{", RegexOptions.Compiled);
        private static readonly Regex ParsingRegex1 = new Regex(@"(?<={{)(?<code>[^\s}]+)(\s+(?<filtername>[\w]+)\((?<args>.*?)\))?(?=}})", RegexOptions.Compiled);
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="code"></param>
        /// <param name="parent"></param>
        /// <param name="output"></param>
        public PRINTNodeContext(string code, NodeBlockContext parent, OutPutProvide output)
            : base(code, parent, output)
        {
            NdType = NodeType.PRINT;
        }
        /// <summary>
        /// 节点信息
        /// </summary>
        public List<PrintModel> Model { get; private set; } = default!;
        /// <summary>
        /// 转换表达式
        /// </summary>
        public override void ConvertToExpression()
        {
            var blockparames = new List<ParameterExpression>(10);
            var inits = new List<Expression>(10);

            foreach (var item in Model)
            {
                switch (item.PtType)
                {
                    case PrintType.String:
                        MethodCallExpression print = PrintProvide.ExpressionPrint(Expression.Constant(StringConvert.Convert1(item.SourcePath)));
                        inits.Add(print);
                        break;
                    case PrintType.Variable:
                        var (param, path) = SearchPath(item.SourcePath);
                        blockparames.Add(param);
                        inits.Add(path);
                        if (string.IsNullOrWhiteSpace(item.FilterName))
                        {
                            print = PrintProvide.ExpressionPrint(param);
                        }
                        else
                        {
                            var argsexpression = new List<Expression>(5);
                            foreach (var arg in item.Args)
                            {
                                if (arg.FdType == FieldType.SourcePath)
                                {
                                    var (parame, init) = SearchPath(arg.FdValue);
                                    blockparames.Add(parame);
                                    inits.Add(init);
                                    argsexpression.Add(parame);
                                }
                                else
                                {
                                    var value = CreateConstExpression(arg.FdType, arg.FdValue);
                                    argsexpression.Add(Expression.Convert(value, typeof(object)));
                                }
                            }
                            print = PrintProvide.ExpressionPrint(param, item.FilterName, argsexpression.ToArray());
                        }
                        inits.Add(print);
                        break;
                    default:
                        break;
                }
            }
            MethodCallExpression printLine = PrintProvide.ExpressionPrintLine();
            inits.Add(printLine);
            NdExpression = Expression.Block(blockparames, inits);
        }
        /// <summary>
        /// 解析
        /// </summary>
        protected override void ParsingModel()
        {
            var builder = new StringBuilder(CodeString);
            var list = new List<PrintModel>(10);
            Model = list;
            MatchPrint(builder, list);
        }
        private void MatchPrint(StringBuilder builder, List<PrintModel> prints)
        {
            /*
             * 1.如果匹配第一个{{位置成功,并且位置大于0,说明开始的语句为String类型
             * 2.如果匹配第一个{{位置成功,并且位置不大于0,说明开始的语句为Variable类型
             * 3.如果匹配失败,说明剩余部分是结尾的字符串,则直接判定为String类型
             */
            var mth = ParsingReges2.Match(builder.ToString());
            if (mth.Success)
            {
                if (mth.Index > 0)
                {
                    prints.Add(new PrintModel
                    {
                        PtType = PrintType.String,
                        SourcePath = builder.ToString().Substring(0, mth.Index),
                    });
                    builder.Remove(0, mth.Index);
                    if (builder.Length > 0) MatchPrint(builder, prints);
                    return;
                }
                else
                {
                    mth = ParsingRegex1.Match(builder.ToString());
                    if (mth.Success)
                    {
                        prints.Add(new PrintModel
                        {
                            PtType = PrintType.Variable,
                            SourcePath = mth.Groups["code"].Value,
                            FilterName = mth.Groups["filtername"].Value,
                            Args = CreateModel(mth.Groups["args"].Value),
                        });
                        builder.Remove(0, mth.Length + 4);
                        if (builder.Length > 0) MatchPrint(builder, prints);
                        return;
                    }
                }
            }
            prints.Add(new PrintModel
            {
                PtType = PrintType.String,
                SourcePath = builder.ToString(),
            });
            static FieldModel[] CreateModel(string code)
            {
                var arr = code.Trim().Split(",", System.StringSplitOptions.RemoveEmptyEntries);
                var fds = new List<FieldModel>(10);
                foreach (var item in arr)
                {
                    string value = item.Trim();
                    fds.Add(new FieldModel
                    {
                        FdType = TypeHelper.GetFieldTypeByValue(ref value),
                        FdValue = value,
                    });
                }
                return fds.ToArray();
            }
        }
    }
}