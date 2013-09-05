using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOTSimulator
{
    public interface IResponse
    {
        bool addResponse(CommandResponse _res);
    }
}
