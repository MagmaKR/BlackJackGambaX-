using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BetFormating
{
    public string FormatBet(int bet)
    {
        if (bet >= 1000000)
        {
            return ($"{bet / 100000.0f:0.##}M");
        }
        else if (bet >= 1000)
        {
            return ($"{bet / 1000.0f:0.##}K");
        }
        return bet.ToString();
    }
}
