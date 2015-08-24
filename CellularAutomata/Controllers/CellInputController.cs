/*
 * The controller for the START SIMULATION page. Validates the user input and sends it to the output controller.
 * Developed by Jason Scott and Tiaan Naude under TapX (pty) Ltd.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CellularAutomata.Controllers
{
    public class CellInputController : Controller
    {
        //
        // GET: /CellInput/

        private static Boolean invalidInput = false;

        // Returns the view (flag for error message dependant on successful validation)
        public ActionResult Index()
        {
            ViewBag.invalid = false;
            if (invalidInput)
            {
                ViewBag.invalid = true;
            }
            return View();
        }

        // Gets the form input and calls the validation function - sending data through for simulation if validation was successful
        [HttpPost]
        public ActionResult StartSim(FormCollection formData)
        {
            // Validates input: if false, return to View with error message
            if (!ValidateInput(formData["Rule"], formData["Steps"], formData["Start"]))
            {
                invalidInput = true;
                return RedirectToAction("Index");
            }

            // Initialise variables by converting respective inputs
            Int16 ruleInt = Convert.ToInt16(formData["Rule"]);
            Int16 stepsInt = Convert.ToInt16(formData["Steps"]);
            string startString = formData["Start"];

            // Convert rule of type integer into a boolean array
            bool[] rules = ruleToBinary(ruleInt);

            // Pad starting state with zeroes to match simulation length
            for (int i = 0; i < stepsInt; i++)
            {
                startString = '0' + startString + '0';
            }

            // Create the structures that will be used to send data
            TempData["startString"] = startString;
            TempData["steps"] = stepsInt.ToString();
            TempData["rules"] = rules;
            TempData["ruleNo"] = ruleInt.ToString();

            invalidInput = false;
            return RedirectToAction("Index", "CellOutput");
        }

        // Function to validate whether user-created input is valid or not
        private Boolean ValidateInput(string rule, string steps, string startString)
        {
            //Validate rule input
            Int16 ruleInt, stepsInt;
            try
            {
                ruleInt = Convert.ToInt16(rule);
            }
            catch
            {
                return false;
            }
            if (ruleInt < 0 || ruleInt > 255)
            {
                return false;
            }

            // Validate steps input
            try
            {
                stepsInt = Convert.ToInt16(steps);
            }
            catch
            {
                return false;
            }
            if (stepsInt < 1 || stepsInt > 512)
            {
                return false;
            }

            // Validate start string input
            foreach (char currentChar in startString)
            {
                if (!(currentChar.Equals('1') || currentChar.Equals('0')))
                {
                    return false;
                }
            }
            return true;
        }

        // Converts rule of type integer into a boolean array
        private bool[] ruleToBinary(Int16 ruleInt)
        {
            bool[] ruleBinary = new bool[8];
            string ruleString = Convert.ToString(ruleInt, 2); // Returns string in binary (base 2)
            int i;
            int stringLength = ruleString.Length;
            for (i = 0; i < 8 - stringLength; i++) // Pad zeroes to the left of binary rule if it is not 8-bits
            {
                ruleString = "0" + ruleString;
            }

            i = 0;
            foreach (char currentChar in ruleString) // Convert binary rule to boolean rule
            {
                ruleBinary[7-i] = (currentChar.Equals('1')) ? true : false;
                i++;
            }
            return ruleBinary;
        }
    }
}
