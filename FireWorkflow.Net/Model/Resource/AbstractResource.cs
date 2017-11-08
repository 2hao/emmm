﻿/**
 * Copyright 2003-2008 非也
 * All rights reserved. 
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation。
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see http://www.gnu.org/licenses. *
 * @author 非也,nychen2000@163.com
 * @Revision to .NET 无忧 lwz0721@gmail.com 2010-02
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireWorkflow.Net.Model.Resource
{
    public class AbstractResource : IResource
    {
        #region 属性
        /// <summary>获取或设置资源的名称</summary>
        public String Name { get; set; }

        /// <summary>获取或设置资源的显示名称</summary>
        public String DisplayName { get; set; }

        /// <summary>获取或设置资源的描述</summary>
        public String Description { get; set; }
        #endregion

        public override String ToString()
        {
            if (!String.IsNullOrEmpty(DisplayName))
            {
                return this.DisplayName;
            }
            else
            {
                return this.Name;
            }
        }
    }
}
