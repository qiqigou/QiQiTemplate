﻿using QiQiTemplate.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QiQiTemplate.Model
{
    /// <summary>
    /// 数据实体
    /// </summary>
    public class DynamicModel
    {
        /// <summary>
        /// 子节点
        /// </summary>
        private Dictionary<string, DynamicModel>? FdDict;
        /// <summary>
        /// 构造
        /// </summary>
        public DynamicModel() { }
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="fdtype"></param>
        public DynamicModel(FieldType fdtype)
        {
            FdType = fdtype;
        }
        /// <summary>
        /// 子节点数量
        /// </summary>
        public int Count { get { return FdDict?.Count ?? 0; } }
        /// <summary>
        /// 名称
        /// </summary>
        public string FdName { get; set; } = string.Empty;
        /// <summary>
        /// 类型
        /// </summary>
        public FieldType FdType { get; }
        /// <summary>
        /// 值
        /// </summary>
        public object? FdValue { get; set; }
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static DynamicModel operator --(DynamicModel field)
        {
            field.FdValue = Convert.ToDecimal(field.FdValue) - 1;
            return field;
        }
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="field1"></param>
        /// <param name="field2"></param>
        /// <returns></returns>
        public static bool operator !=(DynamicModel field1, DynamicModel field2) => field1.FdValue?.ToString() != field2.FdValue?.ToString();
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        public static DynamicModel operator ++(DynamicModel field)
        {
            field.FdValue = Convert.ToDecimal(field.FdValue) + 1;
            return field;
        }
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="field1"></param>
        /// <param name="field2"></param>
        /// <returns></returns>
        public static bool operator <(DynamicModel field1, DynamicModel field2) => Convert.ToDecimal(field1.FdValue) < Convert.ToDecimal(field2.FdValue);
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="field1"></param>
        /// <param name="field2"></param>
        /// <returns></returns>
        public static bool operator <=(DynamicModel field1, DynamicModel field2) => Convert.ToDecimal(field1.FdValue) <= Convert.ToDecimal(field2.FdValue);
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="field1"></param>
        /// <param name="field2"></param>
        /// <returns></returns>
        public static bool operator ==(DynamicModel field1, DynamicModel field2) => field1.FdValue?.ToString() == field2.FdValue?.ToString();
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="field1"></param>
        /// <param name="field2"></param>
        /// <returns></returns>
        public static bool operator >(DynamicModel field1, DynamicModel field2) => Convert.ToDecimal(field1.FdValue) > Convert.ToDecimal(field2.FdValue);
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="field1"></param>
        /// <param name="field2"></param>
        /// <returns></returns>
        public static bool operator >=(DynamicModel field1, DynamicModel field2) => Convert.ToDecimal(field1.FdValue) >= Convert.ToDecimal(field2.FdValue);
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is DynamicModel fd)
            {
                var str1 = FdValue?.ToString();
                var str2 = fd.FdValue?.ToString();
                return str1 == str2;
            }
            return false;
        }
        /// <summary>
        /// 根据名称获取子节点
        /// </summary>
        /// <param name="fdName"></param>
        /// <returns></returns>
        public DynamicModel? Get(string fdName)
        {
            DynamicModel? result = default;
            if (fdName == "Count" && FdType == FieldType.Array)
            {
                return new DynamicModel(FieldType.Int) { FdValue = Count };
            }
            if (FdDict?.TryGetValue(fdName, out result) ?? false)
            {
                return result;
            }
            throw new Exception($"对象没有{fdName}属性");
        }
        /// <summary>
        /// 根据索引获取子节点
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public DynamicModel? Get(int idx)
        {
            if (idx > FdDict?.Count - 1) throw new Exception("索引超出界限");
            return FdDict?.Values.ToArray()[idx];
        }
        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public DynamicModel? Get(decimal idx)
        {
            return Get((int)idx);
        }
        /// <summary>
        /// 获取子节点
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public DynamicModel? Get(DynamicModel model)
        {
            return model.FdValue switch
            {
                object msg when msg is string val => Get(val),
                object msg when msg is int val => Get(val),
                object msg when msg is decimal val => Get(val),
                _ => throw new Exception($"{model.FdValue}不是有效类型"),
            };
        }
        /// <summary>
        /// 重载操作符
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => FdValue?.ToString().GetHashCode() ?? 0;
        /// <summary>
        /// 获取所有子节点
        /// </summary>
        /// <returns></returns>
        public List<DynamicModel>? GetValues()
        {
            return FdDict?.Values.ToList();
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="model"></param>
        public void Set(DynamicModel model)
        {
            if (FdDict == null) FdDict = new Dictionary<string, DynamicModel>(20);
            FdDict.TryAdd(model.FdName, model);
        }
        /// <summary>
        /// 将值转为String
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (FdValue == null && Count > 0) return $"[{string.Join(",", GetValues().Select(x => x.ToString()))}]";
            return FdValue?.ToString() ?? string.Empty;
        }
    }
}