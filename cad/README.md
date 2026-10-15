# OpenFrame V0 CAD

[Open the editable Onshape assembly](https://cad.onshape.com/documents/88508516c82cd93ac8930062/w/22791b0330afc36e773169b8/e/c9b546206102fffeee35c3e1). The document is in the **OpenFrame Prototype CAD** folder and has a Part Studio, Feature Studio, and Assembly. The Assembly contains 19 individually named part instances. `OpenFrameV0.fs` is a local backup of the Feature Studio source.

## Files

| File | What it is |
| --- | --- |
| [`OpenFrame-V0-assembly.step`](OpenFrame-V0-assembly.step) | STEP AP242 export of the full V0 assembly (all 19 PRINT and REFERENCE parts), exported from Onshape on 2026-10-15. Opens in SolidWorks, Fusion 360, FreeCAD, NX, etc. |
| [`OpenFrameV0.fs`](OpenFrameV0.fs) | FeatureScript source for the parametric layout feature. |

## Revise it in Onshape

1. Open **Part Studio 1**, then double-click **OpenFrame V0 layout 1** in the feature list.
2. Adjust bridge width, temple length, OLED-to-lens start, combiner yaw, fit clearance, or purchased-part visibility. The separate solids and Assembly instances update from this layout.
3. For more detailed changes, edit **Feature Studio 1**. Keep the local `.fs` backup in sync with those edits.
4. Copy the document or create an Onshape version before major changes.

Parts prefixed **PRINT** are first-pass printable geometry. Parts prefixed **REFERENCE** are simplified purchased-part envelopes for fit checking, not vendor-exact CAD. The Adafruit Micro Lipo charger stays off the glasses as specified in the assembly notes.

| Part | Model basis | Confirm before fabrication |
| --- | --- | --- |
| Seeed XIAO ESP32S3 Sense, SKU 113991115 | 21 × 17.8 × 15 mm Sense stack envelope | Expansion-board orientation, USB clearance, mounting features |
| 0.96 in SSD1306 OLED | 27 × 28 × 4 mm provisional module envelope | Actual PCB, screen active area, header, cable exit |
| Biconvex lens | 34 mm diameter, 2 mm placeholder thickness | Edge thickness, curvature, optical placement |
| Acrylic combiner | 20 × 25 × 1.016 mm | Final cut size, coating side, safe edge detail |
| 500 mAh LiPo | 35 × 25 × 5 mm | Cell protection board, wire strain relief, swelling allowance |
| Adafruit MiniBoost | 17.8 × 11.3 × 5.6 mm envelope | Connector/solder height and mounting |
| Two tactile buttons | 6 × 6 × 6 mm each | Pin spacing and actuator opening |

**V0 limits:** Face width, temple bend, hinges, lens seat, optical path, fasteners, cable channels, ventilation, and actual OLED board size still need physical measurement and fit tests. The model is a revisable spatial prototype, not a fabrication-validated eyewear design. The assembly preserves each Part Studio solid as its own instance at the modeled position; it currently has no mate features.
