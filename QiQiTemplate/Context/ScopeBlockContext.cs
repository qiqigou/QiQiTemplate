﻿using QiQiTemplate.Model;
using System.Linq.Expressions;

namespace QiQiTemplate.Context
{
    /// <summary>
    /// 范围节点
    /// </summary>
    public class ScopeBlockContext : NodeBlockContext
    {
        /// <summary>
        /// 根数据访问名称
        /// </summary>
        public readonly string RootName = "_data";
        /// <summary>
        /// 构造
        /// </summary>
        public ScopeBlockContext() :
            base("_data", null!, null!)
        {
            Scope.Add("_data", Expression.Parameter(typeof(DynamicModel), "_data"));
        }
        /// <summary>
        /// 根数据
        /// </summary>
        public ParameterExpression Root
        {
            get
            {
                Scope.TryGetValue(RootName, out var val);
                return val;
            }
        }
        /// <summary>
        /// 转换表达式
        /// </summary>
        public override void ConvertToExpression()
        {
            NdExpression = MergeNodes();
        }
    }
}