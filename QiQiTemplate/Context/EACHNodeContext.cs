﻿using QiQiTemplate.Enums;
using QiQiTemplate.Model;
using QiQiTemplate.Provide;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace QiQiTemplate.Context
{
    /// <summary>
    /// each 循环节点
    /// </summary>
    public class EACHNodeContext : NodeBlockContext
    {
        /// <summary>
        /// 解析语法正则
        /// </summary>
        protected static readonly Regex ParsingRegex = new Regex(@"{{#each (?<path>[^\s]+) (?<val>[^\s]+) (?<idx>[^\s]+)}}", RegexOptions.Compiled);
        /// <summary>
        /// 循环索引
        /// </summary>
        protected ParameterExpression? _idx;
        /// <summary>
        /// 循环值
        /// </summary>
        protected ParameterExpression? _val;
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="code"></param>
        /// <param name="parent"></param>
        /// <param name="output"></param>
        public EACHNodeContext(string code, NodeBlockContext parent, OutPutProvide output)
            : base(code, parent, output)
        {
            NdType = NodeType.EACH;
            BuildEachVariable();
        }
        /// <summary>
        /// 节点信息
        /// </summary>
        public EachModel? Model { get; private set; }
        /// <summary>
        /// 转换为表达式
        /// </summary>
        public override void ConvertToExpression()
        {
            var (param, path) = SearchPath(Model!.SourcePath);
            var block = MergeNodes();

            var val = SearchVariable(Model.ValName) as ParameterExpression;
            var idx = SearchVariable(Model.IdxName) as ParameterExpression;

            BinaryExpression init_idx = Expression.Assign(SearchVariable(Model.IdxName), Expression.Constant(0));
            MethodCallExpression init_arr = Expression.Call(param, typeof(DynamicModel).GetMethod("Get", new[] { typeof(int) }), idx);
            BinaryExpression init_val = Expression.Assign(val, init_arr);

            LabelTarget label = Expression.Label();
            MemberExpression count = Expression.Property(param, "Count");
            NdExpression = Expression.Block(
                new[] { param, val, idx },
                init_idx,
                path,
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.LessThan(idx, count),
                        Expression.Block(init_val, block, Expression.PostIncrementAssign(idx)),
                        Expression.Break(label)
                    ),
                    label
                )
            );
        }
        /// <summary>
        /// 解析节点
        /// </summary>
        protected override void ParsingModel()
        {
            var mth = ParsingRegex.Match(CodeString);
            Model = new EachModel
            {
                SourcePath = mth.Groups["path"].Value,
                ValName = mth.Groups["val"].Value,
                IdxName = mth.Groups["idx"].Value,
            };
        }
        private void BuildEachVariable()
        {
            ParameterExpression val = Expression.Variable(typeof(DynamicModel), Model!.ValName);
            ParameterExpression idx = Expression.Variable(typeof(int), Model.IdxName);
            Scope.Add(Model.ValName, val);
            Scope.Add(Model.IdxName, idx);
        }
    }
}