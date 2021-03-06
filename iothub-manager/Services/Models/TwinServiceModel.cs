﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Shared;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class TwinServiceModel
    {
        public string ETag { get; set; }
        public string DeviceId { get; set; }
        public string ModuleId { get; set; }
        public bool IsSimulated { get; set; }
        public Dictionary<string, JToken> DesiredProperties { get; set; }
        public Dictionary<string, JToken> ReportedProperties { get; set; }
        public Dictionary<string, JToken> Tags { get; set; }

        public TwinServiceModel()
        {
        }

        public TwinServiceModel(
            string etag,
            string deviceId,
            Dictionary<string, JToken> desiredProperties,
            Dictionary<string, JToken> reportedProperties,
            Dictionary<string, JToken> tags,
            bool isSimulated)
        {
            this.ETag = etag;
            this.DeviceId = deviceId;
            this.DesiredProperties = desiredProperties;
            this.ReportedProperties = reportedProperties;
            this.Tags = tags;
            this.IsSimulated = isSimulated;
        }

        public TwinServiceModel(Twin twin)
        {
            if (twin != null)
            {
                this.ETag = twin.ETag;
                this.DeviceId = twin.DeviceId;
                this.ModuleId = twin.ModuleId;
                this.Tags = TwinCollectionToDictionary(twin.Tags);
                this.DesiredProperties = TwinCollectionToDictionary(twin.Properties.Desired);
                this.ReportedProperties = TwinCollectionToDictionary(twin.Properties.Reported);
                this.IsSimulated = this.Tags.ContainsKey("IsSimulated") && this.Tags["IsSimulated"].ToString() == "Y";
            }
        }

        public Twin ToAzureModel()
        {
            var properties = new TwinProperties
            {
                Desired = DictionaryToTwinCollection(this.DesiredProperties),
                Reported = DictionaryToTwinCollection(this.ReportedProperties),
            };

            return new Twin(this.DeviceId)
            {
                ETag = this.ETag,
                Tags = DictionaryToTwinCollection(this.Tags),
                Properties = properties
            };
        }

        /*
        JValue:  string, integer, float, boolean
        JArray:  list, array
        JObject: dictionary, object

        JValue:     JToken, IEquatable<JValue>, IFormattable, IComparable, IComparable<JValue>, IConvertible
        JArray:     JContainer, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, IEnumerable
        JObject:    JContainer, IDictionary<string, JToken>, ICollection<KeyValuePair<string, JToken>>, IEnumerable<KeyValuePair<string, JToken>>, IEnumerable, INotifyPropertyChanged, ICustomTypeDescriptor, INotifyPropertyChanging
        JContainer: JToken, IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, IEnumerable, ITypedList, IBindingList, IList, ICollection, INotifyCollectionChanged
        JToken:     IJEnumerable<JToken>, IEnumerable<JToken>, IEnumerable, IJsonLineInfo, ICloneable, IDynamicMetaObjectProvider
        */
        private static Dictionary<string, JToken> TwinCollectionToDictionary(TwinCollection x)
        {
            var result = new Dictionary<string, JToken>();

            if (x == null) return result;

            foreach (KeyValuePair<string, object> twin in x)
            {
                try
                {
                    if (twin.Value is JToken)
                    {
                        result.Add(twin.Key, (JToken) twin.Value);
                    }
                    else
                    {
                        result.Add(twin.Key, JToken.Parse(twin.Value.ToString()));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            return result;
        }

        private static TwinCollection DictionaryToTwinCollection(Dictionary<string, JToken> x)
        {
            var result = new TwinCollection();

            if (x != null)
            {
                foreach (KeyValuePair<string, JToken> item in x)
                {
                    try
                    {
                        result[item.Key] = item.Value;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                }
            }

            return result;
        }
    }
}
