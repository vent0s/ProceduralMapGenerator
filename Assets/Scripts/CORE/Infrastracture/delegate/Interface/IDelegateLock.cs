public interface IDelegateLock 
{
    /*
    /// EN:
    /// the greatest weakness of this design, is that one class implementing this interface, can only attach to one delegation body
    /// we need to figureout how to constrain multiple delegation while making their deregistration seperate
    /// 
    /// CN:
    /// 本方案是用来确保委托-线程安全的，它能保证不管是委托挂载者还是被挂载者在被摧毁前能及时撤销自己关联的委托项
    /// 目前是非常初始的方案，仅能支持单个委拖的挂载，在单例环境下应该可以起到很大作用，
    /// 但如果想要面向多重委托关系的话，就得考虑是否要使用更加复杂的数据结构了
    /// 
    /// 本接口是用来与DelegateBody交互的，当挂载接口的时候，将自己注册到被挂载者的该类实例上
    /// 当自己要被摧毁时，就注销掉自己在被挂载对象上的委托并将自己从被挂载DelegateBody实例上删除
    /// 反之亦然，DelegateBody要被摧毁时，也可以反过来通知所有挂载在自己身上的委托，让它们能自己注销自己
     */

    public void DelegateAttach();

    public void DelegateDetach();

    public bool isDelegateAttached { get; set; }
}
