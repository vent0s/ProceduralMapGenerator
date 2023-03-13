namespace septim.unit
{
    public interface IMeleeUnit
    {
        public int[] meleeAtk { get; set; }                     //白刃战基础杀伤，每个存活单位都会在白刃战接敌的时候去进行攻防判定，如果攻大于防，则扔骰子，视结果决定是否造成软杀伤。必定会造成组织度损伤
        public int[] meleeDef { get; set; }                     //白刃战基础防御
        public int[] meleeArmorPiercing { get; set; }           //白刃战穿甲，如果不能穿甲，则只能达成组织度损伤
        public int[] meleeOrganizeDamage { get; set; }          //白刃战组织度损伤
    }

}

