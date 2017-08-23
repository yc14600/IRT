using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;

namespace IRT
{   
    internal class Program
    {
        public static void Main()
        {
            //var dr = new DataReader();
            var data = DataReader.Read("openml_data.json");

            Console.WriteLine("Rows count:" + data.Length);   
            Console.WriteLine("test data : "+ data[3,3]);
            
        }
    }
    public class DataReader
    {
        
        public static double[,] Read(string file_path)
        {
            using (StreamReader r = new StreamReader(file_path))
            {
                string json = r.ReadToEnd();
                var items = JsonConvert.DeserializeObject<algitem[]>(json);
                Console.WriteLine("test alg name :" + items[0].algorithm);
                return Convert2Mx(items);
            }
        }

        public static double[,] Convert2Mx(algitem[] data)
        {
            var features = data[0].GetType().GetFields(BindingFlags.Public | 
                                              BindingFlags.NonPublic | 
                                              BindingFlags.Instance);
                                              
            var train = new double[data.Length,features.Length-1];
            int i = 0;
            foreach(var d in data){
                int  j = 0;
                foreach(var f in features.Skip(1))
                {
                    //Console.WriteLine(i+", "+j);
                    train[i,j++] = (double)f.GetValue(d);
                    
                    //break;
                }
                i++;
            }
            return train;
        }
    }
    public class algitem
    {
        public string algorithm {get; set;}
        public double breast_w {get;set;}
        public double credit_a {get;set;}
        public double credit_g {get;set;}
        public double diabetes {get;set;}
        public double electricity {get;set;}
        public double gina_agnostic {get;set;}
        public double irish {get;set;}
        public double jm1 {get;set;}
        public double kc1 {get;set;}
        public double kr_vs_kp {get;set;}
        public double MagicTelescope {get;set;}
        public double monks_problems_1 {get;set;}
        public double monks_problems_2 {get;set;}
        public double monks_problems_3 {get;set;}
        public double mozilla4 {get;set;}
        public double mushroom {get;set;}
        public double musk {get;set;}
        public double pc1 {get;set;}
        public double pc3 {get;set;}
        public double pc4 {get;set;}
        public double profb {get;set;}
        public double scene {get;set;}
        public double sick {get;set;}
        public double spambase {get;set;}
        public double sylva_agnostic {get;set;}
        public double tic_tac_toe {get;set;}
    
        public static int num_features {get; set;} = 27;

    }
    
}