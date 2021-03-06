﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Dataweb.NShape;
using Dataweb.NShape.Advanced;
using Dataweb.NShape.GeneralShapes;


namespace BasicTutorial {

	public partial class Form1 : Form {

		private static string GetBasicTutorialPath() {
			string appDir = Path.GetDirectoryName(Path.GetDirectoryName(Application.StartupPath));
			return Path.GetFullPath(Path.Combine(appDir, @"Demo Programs\Tutorials\Basic"));
		}


		public Form1() {
			InitializeComponent();
		}


		private void Form1_Load(object sender, EventArgs e) {
			try {
				// Set path to the sample diagram and the diagram file extension
				string dir = Path.Combine(GetBasicTutorialPath(), "Sample Project");
				xmlStore1.DirectoryName = dir;
				xmlStore1.FileExtension = "nspj";
				// Set the name of the project that should be loaded from the store
				project1.Name = "Circles";
				// Add path to the NShape shape library assemblies to the search paths
				string programFilesDir = Environment.GetEnvironmentVariable(string.Format("ProgramFiles{0}", (IntPtr.Size == sizeof(long)) ? "(x86)" : ""));
				project1.LibrarySearchPaths.Add(Path.Combine(programFilesDir, string.Format("dataweb{0}NShape{0}bin", Path.DirectorySeparatorChar)));
				project1.AutoLoadLibraries = true;
				
				// Open the NShape project
				project1.Open();
				
				// Load the diagram
				display1.LoadDiagram("Diagram 1");
			} catch (Exception exc) {
				MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void fileSaveToolStripMenuItem_Click(object sender, EventArgs e) {
			// Save all modifications to the repository (to a XML file in this demo)
			project1.Repository.SaveChanges();
		}


		private void fileLoadStatisticsToolStripMenuItem_Click(object sender, EventArgs e) {
			// Delete all diagrams (including all their shapes)
			List<Diagram> diagrams = new List<Diagram>(project1.Repository.GetDiagrams());
			for (int i = diagrams.Count - 1; i >= 0; --i)
				project1.Repository.DeleteAll(diagrams[i]);

			// Prepare local variables needed for processing the web statistics file
			Dictionary<string, RectangleBase> shapeDict = new Dictionary<string, RectangleBase>(1000);
			int x = 10;
			int y = 500;
			string statisticsFilePath = Path.Combine(GetBasicTutorialPath(), @"Sample Data\Small.txt");

			// Create a new diagram for visualizing the web statistics
			Diagram diagram = new Diagram("D1");
			// Read the web statistics file line by line and create shapes for visualization
			using (TextReader reader = new StreamReader(statisticsFilePath)) {
				string line = reader.ReadLine();
				while (line != null) {
					int idx1 = line.IndexOf(';');
					int idx2 = line.IndexOf(';', idx1 + 1);
					string url = line.Substring(0, idx1);
					RectangleBase referringShape;
					if (!shapeDict.TryGetValue(url, out referringShape)) {
						// If no shape was created for the referring web page yet, create one.
						// For simplicity, we call the CreateInstance method of the shape type instead of 
						// creating a shape from a template (which is recommended for most cases, see chapter 8)
						referringShape = (RectangleBase)project1.ShapeTypes["Ellipse"].CreateInstance();
						// Set size of the shape
						referringShape.Width = 100;
						referringShape.Height = 60;
						// Set position of the shape (diagram coordinates)
						referringShape.X = x + 50;
						referringShape.Y = y + 50;
						// Set text of the shape
						referringShape.SetCaptionText(0, Path.GetFileNameWithoutExtension(url));
						// Add shape to the diagram
						diagram.Shapes.Add(referringShape);

						// Add shape to the local dictionary
						shapeDict.Add(url, referringShape);
						// Advance position (for the next shape)
						x += 120;
						if (x > 1200) {
							x = 10;
							y += 70;
						}
					}
					url = line.Substring(idx1 + 1, idx2 - idx1 - 1);
					RectangleBase referredShape;
					if (!shapeDict.TryGetValue(url, out referredShape)) {
						// If no shape was created for the referred web page yet, create one.
						// For simplicity, we call the CreateInstance method of the shape type instead of 
						// creating a shape from a template (which is recommended for most cases, see chapter 8)
						referredShape = (RectangleBase)project1.ShapeTypes["Ellipse"].CreateInstance();
						// Set size of the shape
						referredShape.Width = 100;
						referredShape.Height = 60;
						// Set position of the shape (diagram coordinates)
						referredShape.X = x + 50;
						referredShape.Y = y + 50;
						// Set text of the shape
						referredShape.SetCaptionText(0, Path.GetFileNameWithoutExtension(url));
						// Add shape to the diagram
						diagram.Shapes.Add(referredShape);

						// Add shape to the local dictionary
						shapeDict.Add(url, referredShape);
						// Advance position (for the next shape)
						x += 120;
						if (x > 1200) {
							x = 10;
							y += 70;
						}
					}
					// Create a line shape for connecting the "referring web page" shape and the "referred web page" shape
					// For simplicity, we call the CreateInstance method of the shape type instead of 
					// creating a shape from a template (which is recommended for most cases, see chapter 8)
					Polyline arrow = (Polyline)project1.ShapeTypes["Polyline"].CreateInstance();
					// Add shape to the diagram
					diagram.Shapes.Add(arrow);
					// Connect one of the line shape's endings (first vertex) to the referring shape's reference point
					arrow.Connect(ControlPointId.FirstVertex, referringShape, ControlPointId.Reference);
					// Connect the other of the line shape's endings (last vertex) to the referred shape
					arrow.Connect(ControlPointId.LastVertex, referredShape, ControlPointId.Reference);

					// Read next text line
					line = reader.ReadLine();
				}
				reader.Close();
			}

			// Insert created diagram into the repository (otherwise it won't be saved to file)
			cachedRepository1.InsertAll(diagram);
			// Display created diagram
			display1.Diagram = diagram;
		}

	}

}