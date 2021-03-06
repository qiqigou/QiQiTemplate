﻿namespace QiQiTemplate.Filter
{
    /// <summary>
    /// 转为大写
    /// </summary>
    public class ToUpperFilter : IFilter
    {
        /// <summary>
        /// 转为大写
        /// </summary>
        /// <param name="code"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string Filter(object code, object[] args)
        {
            return code?.ToString().ToUpper() ?? string.Empty;
        }
    }
}