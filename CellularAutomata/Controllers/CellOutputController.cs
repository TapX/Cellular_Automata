/*
 * The controller for the RESULTS page. Performs both the simulation and output creation from the input data.
 * Developed by Jason Scott and Tiaan Naude under TapX (pty) Ltd.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Drawing;

namespace CellularAutomata.Controllers
{
    public class CellOutputController : Controller
    {
        //
        // GET: /CellOutput/

        // Calls the propogation function, formats the ASCII output and sends the results to the View
        public ActionResult Index()
        {
            try
            {
                // Recieve input from CellInputController and initialise all variables
                var startString = TempData["StartString"] as string;
                var steps = TempData["steps"] as string;
                var rules = TempData["rules"] as bool[];
                var ruleNo = TempData["ruleNo"] as string;

                // Simulate the cellular automation
                List<string> cellData = simulateCells(startString, Convert.ToInt16(steps), rules);
                List<string> asciiData = new List<string>();

                // Set up the structures used to store the results in a text file
                System.IO.File.WriteAllBytes(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/CellAutoOutput.txt", new byte[0]);
                System.IO.StreamWriter txtOutput = new System.IO.StreamWriter(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/CellAutoOutput.txt", true);

                // Convert results to ASCII-based text (true states are represented by an 'X' and false by a ' ')
                string tempLine;
                foreach (string tempString in cellData)
                {
                    tempLine = tempString.Replace("1", "X").Replace("0", " ") + '\n';
                    asciiData.Add(tempLine);
                    txtOutput.WriteLine(tempLine);
                }

                // Send the results to the View
                ViewBag.asciiData = asciiData;
                ViewBag.rule = ruleNo;
                ViewBag.dispAscii = true;

                // Don't display the ASCII results if simulation length is more than 100 (for the sake of aesthetics)
                if (Convert.ToInt16(steps) > 100)
                {
                    ViewBag.dispAscii = false;
                }

                // Generate and save results as a bitmap image
                Bitmap cellImg = generateBitmap(cellData);
                cellImg.Save(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/cellImg.bmp");

                // General house-keeping
                txtOutput.Close();

                // Return the View
                ViewBag.noSim = false;
                return View();
            }
            catch // Make sure the page doesn't crash if the user ends up on this page without simulation data!
            {
                ViewBag.noSim = true;
                return View();
            }
        }

        // Simulate the cell propogation
        private List<string> simulateCells(string cellStart, Int16 steps, bool[] rules)
        {
            // Set up all variables and intialise them to the relevant inputs
            List<string> cellResults = new List<string>();
            cellResults.Add(cellStart);
            string prevCells = cellStart;
            string currCells = "";
            string tempString;
            int numCells = prevCells.Length - 1;

            // Iterate through simulation steps, following the rule to generate each new level
            for (int i = 0; i < steps; i++)
            {
                /* tempString holds the previous cell and neighbor states in binary (e.g.: 011)
                 * This is converted to an integer, which is used as the index in the boolean rule array.
                 * This tells us if the new state is true or false.
                 */ 
                
                // First cell (has to deal with an 'external' neighbour)
                tempString = '0' + prevCells.Substring(0, 2);
                if (rules[Convert.ToInt32(tempString, 2)])
                {
                    currCells += '1';
                }
                else
                {
                    currCells += '0';
                }

                // Center cells - no weird interactions
                for (int j = 1; j < numCells; j++)
                {
                    tempString = prevCells.Substring(j-1,3);
                    if (rules[Convert.ToInt32(tempString, 2)])
                    {
                        currCells += '1';
                    }
                    else
                    {
                        currCells += '0';
                    }
                }

                // Last cell (has to deal with an 'external' neighbour)
                tempString = prevCells.Substring(numCells-1, 2) + '0';
                if (rules[Convert.ToInt32(tempString, 2)])
                {
                    currCells += '1';
                }
                else
                {
                    currCells += '0';
                }

                cellResults.Add(currCells);
                prevCells = currCells;
                currCells = "";
            }
            return cellResults;
        }

        // Returns the text file (if requested by the View)
        public ActionResult DownloadTxt()
        {
            return File(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/CellAutoOutput.txt", "application/txt", Server.UrlEncode("CellAutoOutput.txt"));
        }

        // Generates a bitmap image based off the ASCII results
        private Bitmap generateBitmap(List<String> cellData)
        {
            int width = cellData[0].Length;
            int height = cellData.Count;

            // Cell states will be represented as 3x3 pixel blocks in the bitmap image
            Bitmap cellImg = new Bitmap(width * 3, height * 3);

            // Iterate through the ASCII results and create the blocks along the way
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (cellData[i][j].Equals('0'))
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            // 'False' states are transparent
                            cellImg.SetPixel(j * 3, i * 3 + k, Color.Transparent);
                            cellImg.SetPixel(j * 3 + 1, i * 3 + k, Color.Transparent);
                            cellImg.SetPixel(j * 3 + 2, i * 3 + k, Color.Transparent);
                        }
                    }
                    else
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            // 'True' states are Steel Blue (TM)
                            cellImg.SetPixel(j * 3, i * 3 + k, Color.SteelBlue);
                            cellImg.SetPixel(j * 3 + 1, i * 3 + k, Color.SteelBlue);
                            cellImg.SetPixel(j * 3 + 2, i * 3 + k, Color.SteelBlue);
                        }
                    }                   
                }
            }
            return cellImg;
        }

        // Returns the bitmap image to the View
        public FileContentResult getBitmap()
        {
            byte[] imgBytes = System.IO.File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory + "/App_Data/cellImg.bmp");
            return new FileContentResult(imgBytes, "image/bmp");
        }
    }
}
