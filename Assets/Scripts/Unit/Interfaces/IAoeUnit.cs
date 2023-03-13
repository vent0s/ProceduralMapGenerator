namespace septim.unit
{
    public interface IAoeUnit
    {
        public int[] aoeRange { get; set; }                     //aoe 范围
        public float[] aoeDamageDecliningRate { get; set; }     //aoe 伤害衰减率
    }
}

