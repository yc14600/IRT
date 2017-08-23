using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using MicrosoftResearch.Infer.Models;
using MicrosoftResearch.Infer;
using MicrosoftResearch.Infer.Utils;
using MicrosoftResearch.Infer.Distributions;
using MicrosoftResearch.Infer.Maths;
using MicrosoftResearch.Infer.Factors;

namespace IRT
{
    
    internal class Program
    {
        public static void Main()
        {
            //var dr = new DataReader();
            var data = DataReader.Read("openml_data.json");

            Console.WriteLine("Rows count:" + data.GetLength(0));   
            Console.WriteLine("test data : "+ data[3,3]);

            var test = new DifficultyAbility();
            test.Run(data);
            
        }
    }
    
	public class DifficultyAbility
	{
		public void Run(double[,] data)
		{
			InferenceEngine engine = new InferenceEngine(new ExpectationPropagation());
			
			Rand.Restart(0);

			int nQuestions = data.GetLength(1);
			int nSubjects = data.GetLength(0);
			//int nChoices = 4;
			Gaussian abilityPrior = new Gaussian(2, .1);
			Gaussian difficultyPrior = new Gaussian(2, .1);
			//Gamma discriminationPrior = Gamma.FromMeanAndVariance(1, 0.01);

			//double[] trueAbility, trueDifficulty, trueDiscrimination;
			//int[] trueTrueAnswer;
			//int[][] data = Sample(nSubjects, nQuestions, nChoices, abilityPrior, difficultyPrior, discriminationPrior,
			//	out trueAbility, out trueDifficulty, out trueDiscrimination, out trueTrueAnswer);

			Range question = new Range(nQuestions).Named("question");
			Range subject = new Range(nSubjects).Named("subject");
			//Range choice = new Range(nChoices).Named("choice");
			var response = Variable.Array<double>(subject,question).Named("response");
			response.ObservedValue = data;

			var ability = Variable.Array<double>(subject).Named("ability");
			ability[subject] = Variable.Random(abilityPrior).ForEach(subject);
			var difficulty = Variable.Array<double>(question).Named("difficulty");
			difficulty[question] = Variable.Random(difficultyPrior).ForEach(question);
			//var discrimination = Variable.Array<double>(question).Named("discrimination");
			//discrimination[question] = Variable.Random(discriminationPrior).ForEach(question);
			//var trueAnswer = Variable.Array<int>(question).Named("trueAnswer");
            //trueAnswer[question] = Variable.DiscreteUniform(choice).ForEach(question);

			using (Variable.ForEach(subject)) {
				using (Variable.ForEach(question)) {
                    //var tsum = ability[subject] + difficulty[question];
					var advantage = (ability[subject] / (ability[subject] + difficulty[question])).Named("advantage");                   
                    //var variance = (ability[subject]*difficulty[question])/(tsum*tsum*(tsum+1));
                    response[subject,question] = Variable.GaussianFromMeanAndVariance(advantage, 0.1);
                    //response[subject,question] = advantage;
					//var advantageNoisy = Variable.GaussianFromMeanAndPrecision(advantage, discrimination[question]).Named("advantageNoisy");
					//var correct = (advantageNoisy > 0).Named("correct");
					//using (Variable.If(correct))
					//	response[subject][question] = trueAnswer[question];
					//using (Variable.IfNot(correct))
                    //    response[subject][question] = Variable.DiscreteUniform(choice);
				}
			}

			engine.NumberOfIterations = 20;
			//subject.AddAttribute(new Sequential());  // needed to get stable convergence
            //question.AddAttribute(new Sequential());  // needed to get stable convergence
            
			var difficultyPosterior = engine.Infer<IList<Gaussian>>(difficulty);
            var dpost = new NormPosteriors[nQuestions];
			for (int q = 0; q < nQuestions; q++) {
				Console.WriteLine("difficulty[{0}] = {1} ", q, difficultyPosterior[q]);
                dpost[q] = new NormPosteriors();
                dpost[q].mean = difficultyPosterior[q].GetMean();
                dpost[q].variance = difficultyPosterior[q].GetVariance();
                //Console.WriteLine("mean = {0}", difficultyPosterior[q].GetMean());
			}
            writePostFile("difficulty.json", dpost);
			
			var abilityPosterior = engine.Infer<IList<Gaussian>>(ability);
            var apost = new NormPosteriors[nSubjects];
			for (int s = 0; s < nSubjects; s++) {
				Console.WriteLine("ability[{0}] = {1} ", s, abilityPosterior[s]);
                apost[s] = new NormPosteriors();
                apost[s].mean = abilityPosterior[s].GetMean();
                apost[s].variance = abilityPosterior[s].GetVariance();
			}
            writePostFile("ability.json", apost);
		}
        internal void writePostFile(string file_path, NormPosteriors[] posts){
            using (StreamWriter file = File.CreateText(file_path))
            {
                JsonSerializer serializer = new JsonSerializer();
                //serialize object directly into file stream
                serializer.Serialize(file, posts);
            }

        }

	}
    public class NormPosteriors{
        public double mean{get; set;}
        public double variance{get; set;}

    }
}

