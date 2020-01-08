// <copyright file="GetData.cs" company="Odisee">
//     Elian Van Cutsem - 2ICT2
// </copyright>
// <author>Elian Van Cutsem</author>

namespace Project2
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using GeoJSON;
    using GeoJSON.Net.Feature;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is used to get all feature collections from geo maps
    /// </summary>
    public class GetData
    {
        /// <summary>
        /// Gets the direct string from the reader
        /// </summary>
        public static string GeoJson { get; private set; }

        /// <summary>
        /// this function reads all file data and puts it in a String
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>the feature collection</returns>
        public static FeatureCollection GetFeatures(string path)
        {
            try
            {
                using (StreamReader r = new StreamReader(path))
                {
                    GeoJson = r.ReadToEnd();
                    FeatureCollection features = JsonConvert.DeserializeObject<FeatureCollection>(GeoJson);
                    return features;
                }
            }
            catch
            {
                Console.WriteLine("No path found");
                FeatureCollection features = new FeatureCollection();
                return features;
            }
        }
    }
}
