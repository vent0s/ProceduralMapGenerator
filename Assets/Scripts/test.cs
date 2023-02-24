using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Run();
    }

    public IFoo CreateBar() => new Bar();
    public IFoo CreateFoo() => new Foo();
    // Update is called once per frame

    public void Run()
    {
        var foo = CreateFoo();
        foo.Print();

        var bar = CreateBar();
        bar.Print();
    }
}



public interface IFoo
{
    public void Print();
}

public class Foo : IFoo
{
    virtual public void Print()
    {
        Debug.Log("IFoo");
    }
}

public class Bar : Foo
{
    override public void Print()
    {
        Debug.Log("Bar");
    }
}
