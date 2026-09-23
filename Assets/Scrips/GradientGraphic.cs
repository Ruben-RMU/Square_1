using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;
using UnityEngine.U2D;

[RequireComponent(typeof(CanvasRenderer))]
[AddComponentMenu("UI/Gradient Graphic", 12)]
public class GradientGraphic : MaskableGraphic, ILayoutElement
{
    private static readonly Vector2[] s_SlicedVertices = new Vector2[4];
    private static readonly Vector2[] s_SlicedUVs = new Vector2[4];

    [SerializeField] private Sprite m_Sprite;
    public Sprite sprite
    {
        get => m_Sprite;
        set
        {
            if (m_Sprite == value) return;

            if (m_Tracked) UnTrackImage();
            m_Sprite = value;
            if (isActiveAndEnabled) TrackImage();

            SetVerticesDirty();
            SetMaterialDirty();
        }
    }

    public override Texture mainTexture { get { return sprite ? sprite.texture : s_WhiteTexture; } }

    [SerializeField] private Color m_TopLeftColor = Color.white;
    public Color topLeftColor
    {
        get => m_TopLeftColor;
        set { m_TopLeftColor = value; SetVerticesDirty(); }
    }

    [SerializeField] private Color m_TopRightColor = Color.white;
    public Color topRightColor
    {
        get => m_TopRightColor;
        set { m_TopRightColor = value; SetVerticesDirty(); }
    }

    [SerializeField] private Color m_BottomLeftColor = Color.white;
    public Color bottomLeftColor
    {
        get => m_BottomLeftColor;
        set { m_BottomLeftColor = value; SetVerticesDirty(); }
    }

    [SerializeField] private Color m_BottomRightColor = Color.white;
    public Color bottomRightColor
    {
        get => m_BottomRightColor;
        set { m_BottomRightColor = value; SetVerticesDirty(); }
    }

    [Range(1, 5)]
    [Tooltip("Increasing this value will make the gradient smoother by generating more vertices for the UI mesh; increase it only when needed")]
    [SerializeField] private int m_GradientSmoothness = 1;
    public int gradientSmoothness
    {
        get => m_GradientSmoothness;
        set { m_GradientSmoothness = Mathf.Max(value, 1); SetVerticesDirty(); }
    }

    [SerializeField] private bool m_UseSlicedSprite = true;
    public bool useSlicedSprite
    {
        get => m_UseSlicedSprite;
        set { m_UseSlicedSprite = value; SetVerticesDirty(); }
    }

    [SerializeField] private bool m_FillCenter = true;
    public bool fillCenter
    {
        get => m_FillCenter;
        set { m_FillCenter = value; SetVerticesDirty(); }
    }

    [SerializeField] private float m_PixelsPerUnitMultiplier = 1f;
    public float pixelsPerUnitMultiplier
    {
        get => m_PixelsPerUnitMultiplier;
        set { m_PixelsPerUnitMultiplier = Mathf.Max(value, 0.01f); SetVerticesDirty(); }
    }

    public float pixelsPerUnit
    {
        get
        {
            float spritePixelsPerUnit = 100;
            if (sprite)
                spritePixelsPerUnit = sprite.pixelsPerUnit;

            float referencePixelsPerUnit = 100;
            if (canvas)
                referencePixelsPerUnit = canvas.referencePixelsPerUnit;

            return pixelsPerUnitMultiplier * spritePixelsPerUnit / referencePixelsPerUnit;
        }
    }

    public bool hasBorder { get { return sprite && sprite.border.sqrMagnitude > 0f; } }

    public override Material material
    {
        get
        {
            if (m_Material != null)
                return m_Material;

            if (sprite && sprite.associatedAlphaSplitTexture != null)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                    return Image.defaultETC1GraphicMaterial;
            }

            return defaultMaterial;
        }
        set { base.material = value; }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        TrackImage();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (m_Tracked)
            UnTrackImage();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        m_GradientSmoothness = Mathf.Max(m_GradientSmoothness, 1);
        m_PixelsPerUnitMultiplier = Mathf.Max(m_PixelsPerUnitMultiplier, 0.01f);
    }
#endif

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = GetPixelAdjustedRect();
        float width = rect.width;
        float height = rect.height;

        Vector4 uv = sprite ? DataUtility.GetOuterUV(sprite) : Vector4.zero;
        Vector2 pivot = rectTransform.pivot;
        float bottomLeftX = -width * pivot.x;
        float bottomLeftY = -height * pivot.y;

        if (useSlicedSprite && hasBorder)
        {
            // Generate sliced Image
            Vector4 padding, inner, border;
            if (sprite)
            {
                padding = DataUtility.GetPadding(sprite);
                inner = DataUtility.GetInnerUV(sprite);
                border = sprite.border / pixelsPerUnit;
            }
            else
            {
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect originalRect = rectTransform.rect;

            for (int axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;

                // The adjusted rect (adjusted for pixel correctness) may be slightly larger than the original rect.
                // Adjust the border to match the rect to avoid small gaps between borders (case 833201).
                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = rect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    padding[axis] *= borderScaleRatio;
                    padding[axis + 2] *= borderScaleRatio;
                }

                float paddingStart = padding[axis];
                float paddingEnd = padding[axis + 2];

                s_SlicedVertices[0][axis] = paddingStart;
                s_SlicedVertices[1][axis] = border[axis];
                s_SlicedVertices[2][axis] = rect.size[axis] - border[axis + 2];
                s_SlicedVertices[3][axis] = rect.size[axis] - paddingEnd;

                s_SlicedVertices[0][axis] += (axis == 0) ? bottomLeftX : bottomLeftY;
                s_SlicedVertices[1][axis] += (axis == 0) ? bottomLeftX : bottomLeftY;
                s_SlicedVertices[2][axis] += (axis == 0) ? bottomLeftX : bottomLeftY;
                s_SlicedVertices[3][axis] += (axis == 0) ? bottomLeftX : bottomLeftY;

                s_SlicedUVs[0][axis] = inner[axis];
                s_SlicedUVs[1][axis] = inner[axis];
                s_SlicedUVs[2][axis] = inner[axis + 2];
                s_SlicedUVs[3][axis] = inner[axis + 2];
            }

            float invWidth = 1f / width;
            float invHeight = 1f / height;

            for (int x = 0; x < 3; x++)
            {
                int x2 = x + 1;

                if (!fillCenter && x == 1)
                    continue;

                for (int y = 0; y < 3; y++)
                {
                    if (!fillCenter && y == 1)
                        continue;

                    int y2 = y + 1;
                    int startIndex = vh.currentVertCount;

                    if (gradientSmoothness <= 1 || x != 1 || y != 1)
                    {
                        Color c1 = GetColorAt((s_SlicedVertices[x].x - bottomLeftX) * invWidth, (s_SlicedVertices[y].y - bottomLeftY) * invHeight);
                        Color c2 = GetColorAt((s_SlicedVertices[x].x - bottomLeftX) * invWidth, (s_SlicedVertices[y2].y - bottomLeftY) * invHeight);
                        Color c3 = GetColorAt((s_SlicedVertices[x2].x - bottomLeftX) * invWidth, (s_SlicedVertices[y2].y - bottomLeftY) * invHeight);
                        Color c4 = GetColorAt((s_SlicedVertices[x2].x - bottomLeftX) * invWidth, (s_SlicedVertices[y].y - bottomLeftY) * invHeight);

                        vh.AddVert(new Vector3(s_SlicedVertices[x].x, s_SlicedVertices[y].y, 0), c1, new Vector2(s_SlicedUVs[x].x, s_SlicedUVs[y].y));
                        vh.AddVert(new Vector3(s_SlicedVertices[x].x, s_SlicedVertices[y2].y, 0), c2, new Vector2(s_SlicedUVs[x].x, s_SlicedUVs[y2].y));
                        vh.AddVert(new Vector3(s_SlicedVertices[x2].x, s_SlicedVertices[y2].y, 0), c3, new Vector2(s_SlicedUVs[x2].x, s_SlicedUVs[y2].y));
                        vh.AddVert(new Vector3(s_SlicedVertices[x2].x, s_SlicedVertices[y].y, 0), c4, new Vector2(s_SlicedUVs[x2].x, s_SlicedUVs[y].y));

                        vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
                        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
                    }
                    else
                    {
                        // Generate multiple small quads in the center
                        float invSmoothness = 1f / gradientSmoothness;

                        float _bottomLeftX = s_SlicedVertices[x].x;
                        float _bottomLeftY = s_SlicedVertices[y].y;
                        float _width = s_SlicedVertices[x2].x - _bottomLeftX;
                        float _height = s_SlicedVertices[y2].y - _bottomLeftY;

                        float _bottomLeftUVx = s_SlicedUVs[x].x;
                        float _bottomLeftUVy = s_SlicedUVs[y].y;
                        float _uvWidth = s_SlicedUVs[x2].x - _bottomLeftUVx;
                        float _uvHeight = s_SlicedUVs[y2].y - _bottomLeftUVy;

                        for (int i = 0; i <= gradientSmoothness; i++)
                        {
                            for (int j = 0; j <= gradientSmoothness; j++)
                            {
                                float normalizedX = j * invSmoothness;
                                float normalizedY = i * invSmoothness;

                                Vector3 _position = new Vector3(_bottomLeftX + _width * normalizedX, _bottomLeftY + _height * normalizedY, 0f);
                                Vector2 _uv = new Vector2(_bottomLeftUVx + _uvWidth * normalizedX, _bottomLeftUVy + _uvHeight * normalizedY);
                                Color _color = GetColorAt((_position.x - bottomLeftX) * invWidth, (_position.y - bottomLeftY) * invHeight);

                                vh.AddVert(_position, _color, _uv);
                            }
                        }

                        for (int i = 0; i < gradientSmoothness; i++)
                        {
                            int firstTriangle = startIndex + i * (gradientSmoothness + 1);
                            for (int j = 0; j < gradientSmoothness; j++)
                            {
                                int triangle = firstTriangle + j;
                                int aboveTriangle = triangle + gradientSmoothness + 1;

                                vh.AddTriangle(triangle, aboveTriangle, aboveTriangle + 1);
                                vh.AddTriangle(aboveTriangle + 1, triangle + 1, triangle);
                            }
                        }
                    }
                }
            }
        }
        else
        {
            // Generate normal Image
            if (gradientSmoothness <= 1)
            {
                // Generate single quad
                vh.AddVert(new Vector3(bottomLeftX, bottomLeftY, 0f), bottomLeftColor, new Vector2(uv.x, uv.y));
                vh.AddVert(new Vector3(bottomLeftX, bottomLeftY + height, 0f), topLeftColor, new Vector2(uv.x, uv.w));
                vh.AddVert(new Vector3(bottomLeftX + width, bottomLeftY + height, 0f), topRightColor, new Vector2(uv.z, uv.w));
                vh.AddVert(new Vector3(bottomLeftX + width, bottomLeftY, 0f), bottomRightColor, new Vector2(uv.z, uv.y));

                vh.AddTriangle(0, 1, 2);
                vh.AddTriangle(2, 3, 0);
            }
            else
            {
                // Generate multiple small quads
                float invSmoothness = 1f / gradientSmoothness;

                for (int i = 0; i <= gradientSmoothness; i++)
                {
                    for (int j = 0; j <= gradientSmoothness; j++)
                    {
                        float normalizedX = j * invSmoothness;
                        float normalizedY = i * invSmoothness;

                        Vector3 _position = new Vector3(bottomLeftX + width * normalizedX, bottomLeftY + height * normalizedY, 0f);
                        Vector2 _uv = new Vector2(uv.z * normalizedX + uv.x * (1f - normalizedX), uv.w * normalizedY + uv.y * (1f - normalizedY));
                        vh.AddVert(_position, GetColorAt(normalizedX, normalizedY), _uv);
                    }
                }

                for (int i = 0; i < gradientSmoothness; i++)
                {
                    int firstTriangle = i * (gradientSmoothness + 1);
                    for (int j = 0; j < gradientSmoothness; j++)
                    {
                        int triangle = firstTriangle + j;
                        int aboveTriangle = triangle + gradientSmoothness + 1;

                        vh.AddTriangle(triangle, aboveTriangle, aboveTriangle + 1);
                        vh.AddTriangle(aboveTriangle + 1, triangle + 1, triangle);
                    }
                }
            }
        }
    }

    private Color GetColorAt(float x, float y)
    {
        Color topLerp = topLeftColor * (1f - x) + topRightColor * x;
        Color bottomLerp = bottomLeftColor * (1f - x) + bottomRightColor * x;
        return bottomLerp * (1f - y) + topLerp * y;
    }

    int ILayoutElement.layoutPriority { get { return 0; } }
    float ILayoutElement.minWidth { get { return 0; } }
    float ILayoutElement.minHeight { get { return 0; } }
    float ILayoutElement.flexibleWidth { get { return -1; } }
    float ILayoutElement.flexibleHeight { get { return -1; } }

    float ILayoutElement.preferredWidth
    {
        get
        {
            if (sprite == null)
                return 0;

            return DataUtility.GetMinSize(sprite).x / pixelsPerUnit;
        }
    }

    float ILayoutElement.preferredHeight
    {
        get
        {
            if (sprite == null)
                return 0;

            return DataUtility.GetMinSize(sprite).y / pixelsPerUnit;
        }
    }

    void ILayoutElement.CalculateLayoutInputHorizontal() { }
    void ILayoutElement.CalculateLayoutInputVertical() { }

    // Whether this is being tracked for Atlas Binding
    private bool m_Tracked = false;

    private static List<GradientGraphic> m_TrackedTexturelessImages = new List<GradientGraphic>();
    private static bool s_Initialized;

    private void TrackImage()
    {
        if (sprite != null && sprite.texture == null)
        {
            if (!s_Initialized)
            {
                SpriteAtlasManager.atlasRegistered += RebuildImage;
                s_Initialized = true;
            }

            m_TrackedTexturelessImages.Add(this);
            m_Tracked = true;
        }
    }

    private void UnTrackImage()
    {
        m_TrackedTexturelessImages.Remove(this);
        m_Tracked = false;
    }

    private static void RebuildImage(SpriteAtlas spriteAtlas)
    {
        for (int i = m_TrackedTexturelessImages.Count - 1; i >= 0; i--)
        {
            GradientGraphic image = m_TrackedTexturelessImages[i];
            if (spriteAtlas.CanBindTo(image.sprite))
            {
                image.SetAllDirty();
                m_TrackedTexturelessImages.RemoveAt(i);
            }
        }
    }
}