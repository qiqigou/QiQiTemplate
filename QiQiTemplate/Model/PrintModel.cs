﻿using System;
using QiQiTemplate.Enums;

namespace QiQiTemplate.Model
{
    /// <summary>
    /// 输出节点实体
    /// </summary>
    public class PrintModel
    {
        /// <summary>
        /// 参数
        /// </summary>
        public FieldModel[] Args { get; set; } = Array.Empty<FieldModel>();
        /// <summary>
        /// 过滤器名称
        /// </summary>
        public string FilterName { get; set; } = string.Empty;
        /// <summary>
        /// 类型
        /// </summary>
        public PrintType PtType { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string SourcePath { get; set; } = string.Empty;
    }
}