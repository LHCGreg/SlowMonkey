using System;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Web.XmlTransform;

namespace SlowMonkey.Tasks
{
    public class TransformXml : Task
    {
        [Required]
        public string Source { get; set; }

        [Required]
        public string Destination { get; set; }

        [Required]
        public string Transform { get; set; }

        public override bool Execute()
        {
            Log.LogMessage("Transforming source XML file \"{0}\" using transform \"{1}\" and saving the result in \"{2}\".",
                Source, Transform, Destination);

            string step = "loading source " + Source;
            try
            {
                using (XmlTransformableDocument sourceFile = new XmlTransformableDocument())
                {
                    sourceFile.PreserveWhitespace = true;
                    sourceFile.Load(Source);

                    step = "loading transform " + Transform;

                    using (XmlTransformation transform = new XmlTransformation(Transform))
                    {
                        step = "applying transform";
                        // This seems to throw an exception instead of returning false if there was an error.
                        // Check it just to be safe.
                        bool success = transform.Apply(sourceFile);
                        if (!success)
                        {
                            Log.LogError("There was an error applying the transform.");
                            return false;
                        }

                        step = "saving to destination " + Destination;
                        sourceFile.Save(Destination);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Error while {0}: {1}", step, ex.Message);
                Log.LogError(errorMessage);
                return false;
            }

            return true;
        }
    }
}