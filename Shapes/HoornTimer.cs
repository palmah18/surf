using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoornIsBoss
{
    class HoornTimer
    {
        private DateTime startTime;
        private DateTime stopTime;
        private static HoornTimer instance;

        public static HoornTimer Instance
        {
            get {
                if (instance == null)
                {
                    instance = new HoornTimer();
                }
                return instance;
            }  
        }

        public void Stop()
        {
            stopTime = System.DateTime.Now;
        }

        public void Start()
        {
            startTime = System.DateTime.Now;
        }

        public void calculateDiff(String text = "")
        {
            TimeSpan span = stopTime - startTime;
            int ms = (int)span.TotalMilliseconds;
            Console.WriteLine(text + ": " + ms);
        }
    }
}
