using System;

public class MotorCtrl
{
  //  private int foreward;
 //   private int backward;
 //   private int left;
 //   private int right;
 //   private int stop;
    private bool enable;

    public MotorCtrl()
	{
        enable   = true;
   //     foreward = 0;
   //     backward = 0;
   //     left     = 0;
   //     right    = 0;
   //     stop     = 0;
	}
    public string setEnable(bool status)
    {
        enable = status;
        return enable?"enabl":"disab";//enable disable
    }
    public string setLeft(int val)
    {
        if (enable) return "l" + val.ToString(); 
        else return null;
    }
    public string setRight(int val)
    {
        if (enable) return "r" + val.ToString();
        else return null;
    }
    public string setForeward(int val)
    {
        if (enable) return "f" + val.ToString();
        else return null;
    }
    public string setBackWard(int val)
    {
        if (enable) return "b" + val.ToString();
        else return null;
    }
    public string setStop(int val)
    {
        if (enable) return "s" + val.ToString();
        else return null;
    }
    public string setTurn(int val)
    {
        if (enable) return "t" + val.ToString();
        else return null;
    }
}
