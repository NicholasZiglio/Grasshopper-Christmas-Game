using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using SharpDX.XInput;
using System.IO;

namespace Game
{
    public class GameComponent : GH_Component
    {
        #region Persistent Variables

        readonly GameController gameController = new GameController(UserIndex.One, 10000.0, 10000.0);
        readonly Player player = new Player(2.0);

        //Grasshopper
        GH_Document GrasshopperDocument;
        GH_Component GrasshopperComponent;
        readonly int scheduleSolutionMilliseconds = 5;

        //Rhino


        //Game
        bool isGameInitialized = false;
        DateTime startTime = DateTime.Now;
        DateTime endTime = DateTime.Now;
        double deltaTime;
        Vector3d gravity = new Vector3d(0.0, 0.0, -9.81);

        #endregion Persistent Variables

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GameComponent()
          : base("Game", "Nickname",
            "Description",
            "Category", "Subcategory")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddMeshParameter("Snowballs", "Snowballs", "Snowballs", GH_ParamAccess.list);
            pManager.AddMeshParameter("SnowmenBodies", "SnowmenBodies", "SnowmenBodies", GH_ParamAccess.list);
            pManager.AddPointParameter("pt", "pt", "pt", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            InitializeComponent();
            

            DA.SetDataList(0, Snowball.SnowballMeshes);
            DA.SetDataList(1, Snowman.SnowmenBodyMeshes);
            DA.SetData(2, Snowman.Snowmen[0].headCollisionPoint);

        }

        //Initialize Grasshopper component & Rhino document
        private void InitializeComponent()
        {
            //Get Component
            if (GrasshopperComponent == null)
            {
                GrasshopperComponent = this;
            }
            //If Component
            if (GrasshopperComponent != null)
            {
                //Get Document
                if (GrasshopperDocument == null)
                {
                    GrasshopperDocument = this.OnPingDocument();
                }
                //If Document
                if (GrasshopperDocument != null)
                {
                    if (!isGameInitialized)
                    {
                        Start();
                        isGameInitialized = true;
                    }
                    //Schedule Solution

                    GrasshopperDocument.ScheduleSolution(scheduleSolutionMilliseconds, Update);
                }
            }
        }

        //Start
        public void Start()
        {
            Snowman.Snowmen.Add(new Snowman(player.Location));
            Snowman.Snowmen.Add(new Snowman(player.Location));
            Snowman.Snowmen.Add(new Snowman(player.Location));
        }

        //Update
        public void Update(GH_Document doc)
        {
            //Expire solution
            GrasshopperComponent.ExpireSolution(false);

            //Updates
            GetDeltaTime();
            gameController.UpdateState();
            player.Update(gameController, deltaTime, gravity);
            Snowball.Update(deltaTime, gravity);
            Snowman.Update(player, deltaTime);
        }

        //Get delta time
        private void GetDeltaTime()
        {
            endTime = DateTime.Now;
            deltaTime = (endTime - startTime).TotalMilliseconds / 1000.0;
            startTime = DateTime.Now;
        }


        //Component Extras
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Properties.Resources.GameIcon;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("99DB7909-BDD6-4C15-8095-D5494C278A4B");


        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            //Materials
            Rhino.Display.DisplayMaterial whiteMaterial = new Rhino.Display.DisplayMaterial(System.Drawing.Color.White);

            //Args
            base.DrawViewportMeshes(args);

            //Render Meshes
            RenderMeshList(args, Snowball.SnowballMeshes, whiteMaterial);
            RenderMeshList(args, Snowman.SnowmenBodyMeshes, whiteMaterial);
        }

        //Render List of Meshes
        public void RenderMeshList(IGH_PreviewArgs args, List<Mesh> meshList, Rhino.Display.DisplayMaterial material)
        {
            foreach (Mesh mesh in meshList)
            {
                args.Display.DrawMeshShaded(mesh, material);
            }
        }
    }
}