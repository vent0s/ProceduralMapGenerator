using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace septim.core
{
    /// <summary>
    /// EN:
    /// this class is for restricting delegation attachment, 
    /// whenever there is another body trying to attach a new method to a delegation,
    /// it have to pass into a delegate body for registration.
    /// As such, we can safly record attached bodies, and when we are going to destroy our delegate body, 
    /// we can safly inform all binding bodies to remove themself from this.
    /// 
    /// Thus, we need a HashSet to record objects that implements IDelegateLock interface,
    /// and need to provide a method to remove all biding delegates by traversing the hash set;
    /// 
    /// CN:
    /// 本方案是用来确保委托-线程安全的，它能保证不管是委托挂载者还是被挂载者在被摧毁前能及时撤销自己关联的委托项
    /// 目前是非常初始的方案，仅能支持单个委拖的挂载，在单例环境下应该可以起到很大作用，
    /// 但如果想要面向多重委托关系的话，就得考虑是否要使用更加复杂的数据结构了
    /// 
    /// 这个类是用来接收委托被注册方需要实例化的类，方便委托被注册方知道自己身上挂了哪些委托
    /// 此类需要与IDelegateLock接口配合使用
    /// </summary>
    public class DelegateBody
    {
        ~DelegateBody()
        {
            RemoveAllDelegations();
        }

        HashSet<IDelegateLock> delegateLocks = new HashSet<IDelegateLock>();

        public bool RegisteringDelegation(IDelegateLock input)
        {
            //Debug.Log("On Registration");
            if (!delegateLocks.Contains(input))
            {
                delegateLocks.Add(input);
                //Debug.Log("Rceiving delegation registration");
                return true;
            }
            else
            {
                //Debug.LogError("DELEGATE ATTACHMENT ERROR");
                return false;
            }
        }

        public bool RemoveDelegation(IDelegateLock input)
        {
            if (delegateLocks.Contains(input))
            {

                delegateLocks.Remove(input);
                //Debug.Log("Detaching delegation registration");
                return true;
            }
            else
            {
                //Debug.LogError("DELEGATE DETACHMENT ERROR");
                return false;
            }
        }

        public void RemoveAllDelegations()
        {
            foreach (IDelegateLock var in delegateLocks)
            {
                if (var != null && var.isDelegateAttached)
                {
                    var.DelegateDetach();
                    var.isDelegateAttached = false;
                }
            }
            delegateLocks.Clear();
        }
    }
}
