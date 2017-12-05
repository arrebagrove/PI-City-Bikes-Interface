using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.PI;
using OSIsoft.AF.UnitsOfMeasure;
using System;
using System.Linq;
using System.Collections.Generic;
using OSIsoft.AF.Data;

namespace PICityBikes.Core
{
    public class PISystemManager
    {
        private PISystem piSystem = null;
        private AFDatabase afDb = null;
        private PIServer piServer = null;
        private List<string> piPointNamesToBeCreated = null;
        private const string cityElementTemplateName = "CityTemplate";
        private const string bikeStationElementTemplateName = "BikeStationTemplate";

        public PISystemManager()
        {

        }
        public void Connect(string piSystemName, string afDatabaseName, string piDataArchiveName)
        {
            piPointNamesToBeCreated = new List<string>();

            try
            {
                piSystem = new PISystems()[piSystemName];
                piSystem.Connect();
                LogManager.Instance.Info("Connected to the AF Server.");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Fatal("Could not connect to AF Server: " + ex.Message);
                throw ex;
            }

            try
            {
                afDb = piSystem.Databases[afDatabaseName];
                if (afDb == null)
                {
                    LogManager.Instance.Info("AF Database not found. Creating new AF Database.");
                    afDb = piSystem.Databases.Add(afDatabaseName);
                }
                else
                {
                    LogManager.Instance.Info("AF Database found.");
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.Fatal("Could not connect to AF Database: " + ex.Message);
                throw ex;
            }


            try
            {
                piServer = new PIServers()[piDataArchiveName];
                piServer.Connect();
                LogManager.Instance.Info("Connected to the PI Data Archive.");
            }
            catch (Exception ex)
            {
                LogManager.Instance.Fatal("Could not connect to PI Data Archive: " + ex.Message);
                throw ex;
            }
        }

        //internal AFElement GetBikeStation(Network network, BikeStation bikeStation, AFElement cityElement)
        //{
        //    return cityElement.Elements[bikeStation.FixedName];
        //}

        public bool CreateCityTemplate()
        {
            if (afDb.ElementTemplates[cityElementTemplateName] == null)
            {
                LogManager.Instance.Debug("Creating CityTemplate...");
                AFElementTemplate cityTemplate = afDb.ElementTemplates.Add(cityElementTemplateName);
                UOM degree = piSystem.UOMDatabase.UOMClasses["Plane Angle"].UOMs["degree"];
                AddAttributeTemplate(cityTemplate, "Company", typeof(string), false, null, string.Empty);
                AddAttributeTemplate(cityTemplate, "Url", typeof(string), false, null, string.Empty);
                AddAttributeTemplate(cityTemplate, "Id", typeof(string), false, null, string.Empty);
                AddAttributeTemplate(cityTemplate, "Latitude", typeof(double), false, degree, string.Empty);
                AddAttributeTemplate(cityTemplate, "Longitude", typeof(double), false, degree, string.Empty);
                AddAttributeTemplate(cityTemplate, "City", typeof(string), false, null, string.Empty);
                AddAttributeTemplate(cityTemplate, "Country", typeof(string), false, null, string.Empty);
                return true;
            }
            else
            {
                LogManager.Instance.Debug("CityTemplate is already created.");
                return false;
            }

        }

        private void AddAttributeTemplate(AFElementTemplate template, string name, Type type, bool hasPiPointDr, UOM uom, string categoryName)
        {
            if (template.AttributeTemplates[name] == null)
            {
                AFAttributeTemplate attributeTemplate = template.AttributeTemplates.Add(name);
                attributeTemplate.Type = type;
                attributeTemplate.DefaultUOM = uom;

                if (hasPiPointDr == true)
                {
                    attributeTemplate.DataReferencePlugIn = piSystem.DataReferencePlugIns["PI Point"];
                    if (attributeTemplate.DefaultUOM == null)
                    {
                        attributeTemplate.ConfigString = @"\\" + piServer.Name + @"\CityBikes_%..\Element%_%Element%_%Attribute%";
                    }
                    else
                    {
                        attributeTemplate.ConfigString = @"\\" + piServer.Name + @"\CityBikes_%..\Element%_%Element%_%Attribute%;UOM=" + attributeTemplate.DefaultUOM.Abbreviation;
                    }
                }
                else
                {
                    attributeTemplate.DataReferencePlugIn = null;
                }

                if (string.IsNullOrEmpty(categoryName) == false)
                {
                    attributeTemplate.Categories.Add(categoryName);
                }

            }

        }

        public bool CreateBikeStationTemplate()
        {
            if (afDb.ElementTemplates[bikeStationElementTemplateName] == null)
            {
                LogManager.Instance.Debug("Creating BikeStationTemplate..");
                AFElementTemplate template = afDb.ElementTemplates.Add(bikeStationElementTemplateName);
                UOM degree = piSystem.UOMDatabase.UOMClasses["Plane Angle"].UOMs["degree"];
                AddAttributeTemplate(template, "Latitude", typeof(double), false, degree, string.Empty);
                AddAttributeTemplate(template, "Longitude", typeof(double), false, degree, string.Empty);
                AddAttributeTemplate(template, "Id", typeof(string), false, null, string.Empty);
                AddAttributeTemplate(template, "Free Bikes", typeof(int), true, null, string.Empty);
                AddAttributeTemplate(template, "Empty Slots", typeof(int), true, null, string.Empty);
                AddAttributeTemplate(template, "Source", typeof(string), false, null, string.Empty);
                template.AttributeTemplates["Source"].SetValue("https://api.citybik.es/v2/", null);
                return true;
            }
            else
            {
                LogManager.Instance.Debug("BikeStationTemplate is already created.");
                return false;
            }

        }

        public void Save()
        {
            afDb.CheckIn();
        }







        public bool CreateBikeStation(BikeStation bikeStation, AFElement cityElement)
        {

            AFElement element = cityElement.Elements[bikeStation.FixedName];
            if (element != null)
            {
                return false;
            }



            try
            {
                element = cityElement.Elements.Add(bikeStation.FixedName, afDb.ElementTemplates[bikeStationElementTemplateName]);
                element.Attributes["Id"].SetValue(new AFValue(bikeStation.id));
                element.Attributes["Longitude"].SetValue(new AFValue(bikeStation.longitude));
                element.Attributes["Latitude"].SetValue(new AFValue(bikeStation.latitude));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public AFElement GetCity(Network network)
        {
            return afDb.Elements[network.FixedName];
        }

        public AFElement CreateCity(Network network)
        {
            AFElement element = afDb.Elements[network.FixedName];
            if (element == null)
            {
                element = afDb.Elements.Add(network.FixedName, afDb.ElementTemplates[cityElementTemplateName]);
                element.Attributes["Company"].SetValue(new AFValue(network.CompanyString));
                element.Attributes["Url"].SetValue(new AFValue(network.Href));
                element.Attributes["Id"].SetValue(new AFValue(network.Id));
                element.Attributes["Longitude"].SetValue(new AFValue(network.Location.Longitude));
                element.Attributes["Latitude"].SetValue(new AFValue(network.Location.Latitude));
                element.Attributes["City"].SetValue(new AFValue(network.Location.City));
                element.Attributes["Country"].SetValue(new AFValue(network.Location.Country));
            }
            return element;
        }

        public void CreateAllPIPoints(List<string> newPIPointNames)
        {
            if (newPIPointNames.Count > 0)
            {
                IEnumerable<PIPoint> allPIPoints = PIPoint.FindPIPoints(piServer, "*");
                IEnumerable<string> allPIPointNames = allPIPoints.Select(p => p.Name);

                List<string> filteredNewPIPointNames = newPIPointNames.ToList();
                //.Where(p => allPIPointNames.Contains(p) == false).ToList();


                IDictionary<string, object> attributes = new Dictionary<string, object>();
                attributes.Add("pointtype", "int32");
                piServer.Connect();
                AFListResults<string, PIPoint> results = piServer.CreatePIPoints(filteredNewPIPointNames.AsEnumerable(), attributes);
            }
        }

        public void UpdateValues(AFValues cityValues)
        {
            AFErrors<AFValue> errors = AFListData.UpdateValues(cityValues, AFUpdateOption.NoReplace);
        }


        public AFValues GetBikeStationValues(BikeStation bikeStation, AFElement element)
        {
            AFValues values = new AFValues();
            if (element.Attributes["Free Bikes"] != null)
            {
                values.Add(new AFValue(element.Attributes["Free Bikes"], bikeStation.free_bikes, new OSIsoft.AF.Time.AFTime(bikeStation.timestamp)));
            }
            if (element.Attributes["Empty Slots"] != null)
            {
                values.Add(new AFValue(element.Attributes["Empty Slots"], bikeStation.empty_slots, new OSIsoft.AF.Time.AFTime(bikeStation.timestamp)));
            }
            return values;
        }

    }
}
