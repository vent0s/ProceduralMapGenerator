namespace septim.unit
{
    /// <summary>
    /// 这是为平射单位准备的接口，如果弹药充足，这些单位可以自动攻击踏入它们攻击范围的目标，每次攻击都会消耗一次弹药，弹药耗尽后需要在自己的回合装填
    /// </summary>
    public interface IRangedUnit
    {
        public int[] attackRange { get; set; }

        public int[] rangeAccurecy //基础远程命中率，会受到地形影响
        {
            get; set;
        }
        public int[] ammo //弹药，类少女前线
        {
            get; set;
        }
        public int[] rangedArmorPiercing { get; set; }      //远程穿甲

        public int[] rangedOrganizeDamage { get; set; }     //远程组织度损伤

        public void ReloadAmmo();
    }
}


