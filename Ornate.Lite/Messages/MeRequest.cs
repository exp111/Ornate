using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ornate.Lite.Messages
{
    public class MeRequest // GET /api/me/?w=<width?>&v=<version>&x=<timestamp>&lang=en
    {
       //TODO: timestamp and language code are on (almost?) every message
       //      exception: during POSTs timestamp in url, lang in POST
    }
}
