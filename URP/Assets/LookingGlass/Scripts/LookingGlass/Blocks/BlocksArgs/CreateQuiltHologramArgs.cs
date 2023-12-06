﻿using System;

namespace LookingGlass.Blocks {
    /// <summary>
    /// <para>A struct that contains the args for a <c>createQuiltHologram</c> Blocks API call.</para>
    /// <para>See also: <seealso href="https://blocks.glass/api/graphql"/></para>
    /// </summary>
    [Serializable]
    public struct CreateQuiltHologramArgs {
        public static CreateQuiltHologramArgs Create1x1Args() =>
            new CreateQuiltHologramArgs {
                title = "Dummy 1x1 Test Quilt",
                description = "This is a test 1x1 quilt.",
                width = 1,
                height = 1,
                type = HologramType.QUILT,
                aspectRatio = 0.75f,
                quiltCols = 1,
                quiltRows = 1,
                quiltTileCount = 1,
                isPublished = true,
                privacy = PrivacyType.UNLISTED
            };

        public string title;
        public string description;

        /// <summary>
        /// NOTE: This field is automatically set during the upload process.
        /// </summary>
        public string imageUrl;

        public int width;
        public int height;
        public HologramType type;

        /// <summary>
        /// NOTE: This field is automatically set during the upload process.
        /// </summary>
        public int fileSize;

        public float aspectRatio;
        public int quiltCols;
        public int quiltRows;
        public int quiltTileCount;
        public bool isPublished;
        public PrivacyType privacy;

        public CreateQuiltHologramArgs(HologramRenderSettings renderSettings) : this() {
            CopyFrom(renderSettings);
        }

        public void CopyFrom(HologramRenderSettings renderSettings) {
            width = renderSettings.quiltWidth;
            height = renderSettings.quiltHeight;
            type = HologramType.QUILT;
            quiltCols = renderSettings.viewColumns;
            quiltRows = renderSettings.viewRows;
            quiltTileCount = renderSettings.numViews;
            aspectRatio = renderSettings.aspect;
        }
    }
}
