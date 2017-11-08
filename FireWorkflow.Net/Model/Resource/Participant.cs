/**
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
    /// <summary>参与者。</summary>
    public class Participant : AbstractResource
    {

        /// <summary>
        /// 任务分配句柄的类名。<br/>
        /// Fire workflow引擎调用该句柄获得真正的操作者ID。
        /// </summary>
        public String AssignmentHandler { get; set; }

        public Participant(String name)
        {
            this.Name = name;
        }
    }
}
