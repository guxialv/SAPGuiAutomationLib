﻿using SAPFEWSELib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SAPAutomation.Extension;
using SAPAutomation.Framework.Attributes;
using SAPAutomation.Data;
using System.Data;

namespace SAPAutomation
{
    
    public class Test
    {
        [SAPBizData]
        public string TCode { get; set; }

        public void TestFunc()
        {
            
        }

        [SAPBizData]
        public ClassB CB { get; set; }
    }

    public class ClassB
    {
        public string Name { get; set; }
    }
}
