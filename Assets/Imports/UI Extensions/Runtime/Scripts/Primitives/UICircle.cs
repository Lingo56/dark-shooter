using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
    [AddComponentMenu("UI/Extensions/Primitives/UI Circle")]
    public class UICircle : UIPrimitiveBase
    {
        [Tooltip("The Arc Invert property will invert the construction of the Arc.")]
        public bool ArcInvert = true;

        [Tooltip("The Arc property is a percentage of the entire circumference of the circle.")]
        [Range(0, 1)]
        public float Arc = 1;

        [Tooltip("The Arc Steps property defines the number of segments that the Arc will be divided into.")]
        [Range(0, 1000)]
        public int ArcSteps = 100;

        [Tooltip("The Arc Rotation property permits adjusting the geometry orientation around the Z axis.")]
        [Range(0, 360)]
        public int ArcRotation = 0;

        [Tooltip("The Progress property allows the primitive to be used as a progression indicator.")]
        [Range(0, 1)]
        public float Progress = 0;

        private float _progress = 0;

        public Color ProgressColor = new Color(255, 255, 255, 255);
        public bool Fill = true; //solid circle
        public float Thickness = 5;
        public int Padding = 0;

        [Tooltip("Enable or disable the outline around the circle.")]
        public bool EnableOutline = true;

        [Tooltip("Outline thickness around the circle.")]
        public float OutlineThickness = 2;

        [Tooltip("Outline color around the circle.")]
        public Color OutlineColor = Color.black;

        private List<int> indices = new List<int>();  //ordered list of vertices per tri
        private List<UIVertex> vertices = new List<UIVertex>();
        private Vector2 uvCenter = new Vector2(0.5f, 0.5f);

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            int _inversion = ArcInvert ? -1 : 1;
            float Diameter = (rectTransform.rect.width < rectTransform.rect.height ? rectTransform.rect.width : rectTransform.rect.height) - Padding; //correct for padding and always fit RectTransform
            float outerDiameter = -rectTransform.pivot.x * Diameter;
            float innerDiameter = -rectTransform.pivot.x * Diameter + Thickness;

            vh.Clear();
            indices.Clear();
            vertices.Clear();

            int i = 0;
            int j = 1;
            int k = 0;

            float stepDegree = (Arc * 360f) / ArcSteps;
            _progress = ArcSteps * Progress;
            float rad = _inversion * Mathf.Deg2Rad * ArcRotation;
            float X = Mathf.Cos(rad);
            float Y = Mathf.Sin(rad);

            var vertex = UIVertex.simpleVert;
            vertex.color = _progress > 0 ? ProgressColor : color;

            //initial vertex
            vertex.position = new Vector2(outerDiameter * X, outerDiameter * Y);
            vertex.uv0 = new Vector2(vertex.position.x / Diameter + 0.5f, vertex.position.y / Diameter + 0.5f);
            vertices.Add(vertex);

            var iV = new Vector2(innerDiameter * X, innerDiameter * Y);
            if (Fill) iV = Vector2.zero; //center vertex to pivot
            vertex.position = iV;
            vertex.uv0 = Fill ? uvCenter : new Vector2(vertex.position.x / Diameter + 0.5f, vertex.position.y / Diameter + 0.5f);
            vertices.Add(vertex);

            for (int counter = 1; counter <= ArcSteps; counter++)
            {
                rad = _inversion * Mathf.Deg2Rad * (counter * stepDegree + ArcRotation);
                X = Mathf.Cos(rad);
                Y = Mathf.Sin(rad);

                vertex.color = counter > _progress ? color : ProgressColor;
                vertex.position = new Vector2(outerDiameter * X, outerDiameter * Y);
                vertex.uv0 = new Vector2(vertex.position.x / Diameter + 0.5f, vertex.position.y / Diameter + 0.5f);
                vertices.Add(vertex);

                //add additional vertex if required and generate indices for tris in clockwise order
                if (!Fill)
                {
                    vertex.position = new Vector2(innerDiameter * X, innerDiameter * Y);
                    vertex.uv0 = new Vector2(vertex.position.x / Diameter + 0.5f, vertex.position.y / Diameter + 0.5f);
                    vertices.Add(vertex);
                    k = j;
                    indices.Add(i);
                    indices.Add(j + 1);
                    indices.Add(j);
                    j++;
                    i = j;
                    j++;
                    indices.Add(i);
                    indices.Add(j);
                    indices.Add(k);
                }
                else
                {
                    indices.Add(i);
                    indices.Add(j + 1);
                    //Fills (solid circle) with progress require an additional vertex to
                    // prevent the base circle from becoming a gradient from center to edge
                    if (counter > _progress)
                    {
                        indices.Add(ArcSteps + 2);
                    }
                    else
                    {
                        indices.Add(1);
                    }

                    j++;
                    i = j;
                }
            }

            //this vertex is added to the end of the list to simplify index ordering on geometry fill
            if (Fill)
            {
                vertex.position = iV;
                vertex.color = color;
                vertex.uv0 = uvCenter;
                vertices.Add(vertex);
            }

            // Draw the outlines if enabled
            if (EnableOutline && OutlineThickness > 0)
            {
                // Outside outline
                DrawOutline(vh, outerDiameter, outerDiameter + OutlineThickness, _inversion, stepDegree, ArcRotation, OutlineColor);

                // Inside outline
                DrawOutline(vh, innerDiameter - OutlineThickness, innerDiameter, _inversion, stepDegree, ArcRotation, OutlineColor);
            }

            vh.AddUIVertexStream(vertices, indices);
        }

        private void DrawOutline(VertexHelper vh, float innerDiameter, float outerDiameter, int inversion, float stepDegree, int arcRotation, Color outlineColor)
        {
            int outlineStartIndex = vertices.Count;

            for (int counter = 0; counter <= ArcSteps; counter++)
            {
                float rad = inversion * Mathf.Deg2Rad * (counter * stepDegree + arcRotation);
                float X = Mathf.Cos(rad);
                float Y = Mathf.Sin(rad);

                var vertex = UIVertex.simpleVert;
                vertex.color = outlineColor;

                // Outer vertex
                vertex.position = new Vector2(outerDiameter * X, outerDiameter * Y);
                vertex.uv0 = new Vector2(vertex.position.x / outerDiameter + 0.5f, vertex.position.y / outerDiameter + 0.5f);
                vertices.Add(vertex);

                // Inner vertex
                vertex.position = new Vector2(innerDiameter * X, innerDiameter * Y);
                vertex.uv0 = new Vector2(vertex.position.x / innerDiameter + 0.5f, vertex.position.y / innerDiameter + 0.5f);
                vertices.Add(vertex);

                if (counter > 0)
                {
                    indices.Add(outlineStartIndex + (counter - 1) * 2);
                    indices.Add(outlineStartIndex + counter * 2);
                    indices.Add(outlineStartIndex + (counter - 1) * 2 + 1);

                    indices.Add(outlineStartIndex + counter * 2);
                    indices.Add(outlineStartIndex + counter * 2 + 1);
                    indices.Add(outlineStartIndex + (counter - 1) * 2 + 1);
                }
            }
        }

        // The following methods may be used during run-time
        // to update the properties of the component
        public void SetProgress(float progress)
        {
            Progress = progress;
            SetVerticesDirty();
        }

        public void SetArc(float arc)
        {
            Arc = arc;
            SetVerticesDirty();
        }

        public void SetArcSteps(int steps)
        {
            ArcSteps = steps;
            SetVerticesDirty();
        }

        public void SetInvertArc(bool invert)
        {
            ArcInvert = invert;
            SetVerticesDirty();
        }

        public void SetArcRotation(int rotation)
        {
            ArcRotation = rotation;
            SetVerticesDirty();
        }

        public void SetFill(bool fill)
        {
            Fill = fill;
            SetVerticesDirty();
        }

        public void SetBaseColor(Color color)
        {
            this.color = color;
            SetVerticesDirty();
        }

        public void UpdateBaseAlpha(float value)
        {
            var _color = this.color;
            _color.a = value;
            this.color = _color;
            SetVerticesDirty();
        }

        public void SetProgressColor(Color color)
        {
            ProgressColor = color;
            SetVerticesDirty();
        }

        public void UpdateProgressAlpha(float value)
        {
            ProgressColor.a = value;
            SetVerticesDirty();
        }

        public void SetPadding(int padding)
        {
            Padding = padding;
            SetVerticesDirty();
        }

        public void SetThickness(int thickness)
        {
            Thickness = thickness;
            SetVerticesDirty();
        }

        public void SetEnableOutline(bool enable)
        {
            EnableOutline = enable;
            SetVerticesDirty();
        }

        public void SetOutlineThickness(float thickness)
        {
            OutlineThickness = thickness;
            SetVerticesDirty();
        }

        public void SetOutlineColor(Color color)
        {
            OutlineColor = color;
            SetVerticesDirty();
        }
    }
}