namespace septim.unit
{
    public interface IMartialUnit
    {
        public bool[] canAttack { get; set; }
        public int[] supply { get; set; }                   //口粮，类少女前线
        public int[] manPower { get; set; }                 //人员，每个人员代表一次命中判定，人员死完这个编制就消失，命中即人员必定死亡
        public int[] organizationMax { get; set; }          //最大组织度
        public int[] organization { get; set; }             //组织度，类钢铁雄心
        public int[] organizeRecorver { get; set; }         //组织度恢复速度
    }
}

