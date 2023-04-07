using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ornate.Lite.Messages.WS
{
    [WSMessageData("getstate")]
    public class GetStateWSMessageData : BaseWSMessageData
    {
    }

    [WSResponseData("getstate")]
    public class GetStateWSResponseData : BaseWSResponseData
    {
    }
}
