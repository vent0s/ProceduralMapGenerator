namespace septim.unit
{
    /// <summary>
    /// 这是为火炮等火力投射单位准备的接口，火炮无法像平射单位那样自动攻击射程内的目标，且无法精确命中目标，只能在命中范围内随机命中。
    /// 因为抛射会和平射距离不同，所以火炮单位也需要有一个属于自己的射程。
    /// 火炮单位没有命中率，它们的布朗运动弹道就是它们的命中率。
    /// 由于弓箭手也可以是抛射单位，所以抛射单位也应该有穿甲判定
    /// </summary>
    public interface IProjectileUnit
    {
        public int[] projectileAttackRange { get; set; }
        public int[] projectileHitRange { get; set; }
        public int[] projectileArmorPiercing { get; set; }
    }
}
