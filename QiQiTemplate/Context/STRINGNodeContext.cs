﻿using QiQiTemplate.Enums;
using QiQiTemplate.Provide;
using QiQiTemplate.Tool;
using System.Linq.Expressions;

namespace QiQiTemplate.Context
{
    /// <summary>
    /// String 节点
    /// </summary>
    public class STRINGNodeContext : NodeContext
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="code"></param>
        /// <param name="parent"></param>
        /// <param name="output"></param>
        public STRINGNodeContext(string code, NodeBlockContext parent, OutPutProvide output) :
            base(code, parent, output)
        {
            NdType = NodeType.STRING;
        }
        /// <summary>
        /// 转为表达式
        /// </summary>
        public override void ConvertToExpression()
        {
            NdExpression = PrintProvide.ExpressionPrintLine(Expression.Constant(StringConvert.Convert1(CodeString)));
        }
    }
}