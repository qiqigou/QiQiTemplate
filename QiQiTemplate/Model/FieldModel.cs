﻿using QiQiTemplate.Enums;

namespace QiQiTemplate.Model
{
    /// <summary>
    /// 字段实体
    /// </summary>
    public class FieldModel
    {
        /// <summary>
        /// 字段类型
        /// </summary>
        public FieldType FdType { get; set; }
        /// <summary>
        /// 字段值
        /// </summary>
        public string FdValue { get; set; } = string.Empty;
    }
}