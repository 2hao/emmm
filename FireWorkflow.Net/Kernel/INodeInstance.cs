using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FireWorkflow.Net.Kernel.Event;
using FireWorkflow.Net.Model;

namespace FireWorkflow.Net.Kernel
{

    /// <summary>
    /// NodeInstance应该是无状态的，不会随着ProcessInstance的增加而增加。??)
    /// </summary>
    public interface INodeInstance
    {
        String Id { get; }
        /// <summary>node 触发 (最核心的方法) </summary>
        /// <param name="token"></param>
        void fire(IToken token);// throws KernelException;

        /// <summary>获取输出弧的实例</summary>
        /// <returns></returns>
        List<ITransitionInstance> LeavingTransitionInstances { get; set; }
        void AddLeavingTransitionInstance(ITransitionInstance transitionInstance);

        List<ITransitionInstance> EnteringTransitionInstances { get; set; }
        void AddEnteringTransitionInstance(ITransitionInstance transitionInstance);

        List<ILoopInstance> LeavingLoopInstances { get; set; }
        void AddLeavingLoopInstance(ILoopInstance loopInstance);

        List<ILoopInstance> EnteringLoopInstances { get; set; }
        void AddEnteringLoopInstance(ILoopInstance loopInstance);

        List<INodeInstanceEventListener> EventListeners { get; set; }


    }
}
