﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace jenkins_client
{
    public class Build : LazyObject<JObject>
    {
        public static readonly int PollingInterval = 1000;

        protected Client client { get; set; }

        public int number { get; protected set; }
        public string url { get; private set; }
        public Job job { get; private set; }
        
        public bool building
        {
            get
            {
                EnsureDataInLocal();

                return (bool)data[nameof(building)];
            }
        }
        public int estimatedDuration
        {
            get
            {
                EnsureDataInLocal();

                return (int)data[nameof(estimatedDuration)];
            }
        }
        public int duration
        {
            get
            {
                EnsureDataInLocal();

                return (int)data[nameof(duration)];
            }
        }
        public long timestamp
        {
            get
            {
                EnsureDataInLocal();

                return Convert.ToInt64(data[nameof(timestamp)]);
            }
        }
        public string result
        {
            get
            {
                EnsureDataInLocal();

                return (string)data[nameof(estimatedDuration)];
            }
        }

        internal Build(Job job, int number, string url)
        {
            this.job = job;
            this.number = number;
            this.url = url;

            this.client = job.client;
        }

        public override async Task<JObject> Fetch()
        {
            var response = await client.api.GetBuildData(job.name, number);

            return JObject.Parse(response.body);
        }

        private async Task<bool> WaitForBuildEnd(Timeout timeout)
        {
            if (PollingInterval > timeout.remaining)
                Console.WriteLine("");

            while (!timeout.isExpired)
            {
                Invalidate();

                if (!building)
                    return true;

                await Task.Delay(PollingInterval);
            }

            return false;
        }
        public async Task<bool> WaitForBuildEnd(int timeout)
        {
            return await WaitForBuildEnd(new Timeout(timeout));
        }
        public async Task WaitForBuildEnd()
        {
            await WaitForBuildEnd(new Timeout(Timeout.Infinite));
        }
        public async Task<bool> WaitForBuildEnd(CancellationToken ct)
        {
            return await WaitForBuildEnd(new Timeout(ct));
        }
    }
}