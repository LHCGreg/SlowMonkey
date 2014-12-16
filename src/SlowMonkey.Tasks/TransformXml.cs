using System;
using System.Xml;
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

                    using (XmlTextReader sourceReader = new XmlTextReader(Source))
                    {
                        // Don't call use sourceFile.Load(Source) because that triggers a mono bug where
                        // the (private) property sourceFile.TextEncoding is always set to Unicode.
                        // This is a result of mono bug https://bugzilla.xamarin.com/show_bug.cgi?id=25401
                        // Constructing our own XmlTextReader, passing in the file name instead of a TextReader,
                        // works around this bug. Otherwise transformed config files will be encoded in UTF-16 instead of
                        // UTF-8 if the source file is UTF-8 but does not have a UTF-8 BOM.
                        //sourceFile.Load(Source);
                        sourceFile.Load(sourceReader);
                    }

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