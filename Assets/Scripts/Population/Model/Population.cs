using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace septim.population
{
    /// <summary>
    /// EN:
    /// For population, we might need to introduce inheritance to apply polymophism due to different bahvior from different classes and jobs.
    /// For example, citizens with bourgeoisies ideology might willing to become upper class, if they have enough budget, they are willing to perform investment or start a business.
    /// Yet for upperclass, they will always performing investment or business running, even if they have non-bourgeoisies ideology.
    /// (I know this is not reasonable, it is a tradeoff to implement robust gameplay)
    /// 
    /// Also, pre-mordern populations will have different sets of behaviours, such as pesant, slave, aristocrats and clergy,
    /// Meanwhile, there will also have a huge variation in sorcerer class, which need to be defined carefully.
    /// 
    /// For ideology, there are way more issues to be figured out, such as...non-bourgeoisies ideology sometime might be capatable with bourgeoisies ideology, such as some pre-modern ideology,
    /// I have to figured out a best solution for catagorization.
    /// 
    /// CN:
    /// 我们需要对人口引入多态概念，因为不同阶级的人会有不同的行为。
    /// 比如：具有布尔乔亚意识形态的人，往往更倾向于去投资或创业，但上层阶级哪怕没有布尔乔亚意识形态也会主动投资和创业。（我也知道这很蛇皮，但为了系统的稳定和简洁性，也只能暂时这么整了）
    /// 此外，前现代人口的行为和现代工业人口的行为又有更大的区别，比如农奴，自耕农，地主，旧贵族和教士。
    /// 同时，法师阶级也具有极大的分歧，因为工业社会的需求，教育的普及是必然的，一个国家想要强大，法术知识就不可能垄断在少部分人手中（虽然核心知识还是会被垄断）
    /// 法师不可避免地会分裂为反动和进步势力，因此我们需要更加谨慎的去定义法师阶级的行为
    /// 
    /// 关于意识形态，这里可能会有更加复杂的机制，因为某些非布尔乔亚意识形态也会和布尔乔亚意识形态共存，比如某些前现代意识形态等，所以在分类学上我可能得花点功夫了。
    /// 最简单的方法就是照着法哲学原理怼了，也许[闭合-中心-敞开]这种也不错？
    /// </summary>
    public class Population
    {
        public int populationId { get; private set; }

        //type

        //need

        //desire

        //wealth

        //ideology

    }
}

