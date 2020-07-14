﻿using System;
using System.Collections.Generic;
using System.Text;

namespace QiQiTemplate.Filter
{
    /// <summary>
    /// 向右补位
    /// </summary>
    public class PadRightFilter : IFilter
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name => "padright";
        /// <summary>
        /// 过滤
        /// </summary>
        /// <param name="code"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public string Filter(object code, object[] args)
        {
            int width = Convert.ToInt32(args[0]);
            char padding = Convert.ToChar(args[1]);
            return code.ToString().PadRight(width, padding);
        }
    }
}
