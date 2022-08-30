using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DebugxLog.Tools
{
    /// <summary>
    /// 事件处理器
    /// 提供了绑定，解绑，调用等方法。
    /// 没有提供Clear，因为每个人都应当管理自己需要绑定和解绑的委托。此处理器就是为了屏蔽action=null等较危险的操作权限
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ActionHandler<T>
    {
        //不能<in T>来应用于所有数量参数的Action，只能多写几个了
        //有其他数量参数的Action就复制这个类改一下

        private event Action<T> MainAction;

        /// <summary>
        /// 绑定委托
        /// </summary>
        /// <param name="action"></param>
        /// <param name="preventDuplicate"></param>
        public void Bind(Action<T> action, bool preventDuplicate = true)
        {
            if (preventDuplicate && DebugxTools.ContainsDelegate(MainAction, action)) return;

            MainAction += action;
        }

        /// <summary>
        /// 解绑委托
        /// </summary>
        /// <param name="action"></param>
        public void Unbind(Action<T> action)
        {
            MainAction -= action;
        }

        /// <summary>
        /// 调用
        /// </summary>
        /// <param name="obj"></param>
        public void Invoke(T obj)
        {
            if (MainAction == null) return;
            MainAction.Invoke(obj);
        }

        //private void Clear()
        //{
        //    MainAction = null;
        //}
    }
}
