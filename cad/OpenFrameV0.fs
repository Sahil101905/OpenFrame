FeatureScript 3095;
import(path : "onshape/std/common.fs", version : "3095.0");

// OpenFrame V0: editable layout and component envelopes, in millimeters.
// The optical geometry is a starting fixture, not a verified prescription.
// One custom feature makes separate Part Studio solids for assembly insertion.

const BRIDGE_BOUNDS = { (millimeter) : [8, 14, 25] } as LengthBoundSpec;
const TEMPLE_BOUNDS = { (millimeter) : [100, 135, 175] } as LengthBoundSpec;
const OPTICAL_BOUNDS = { (millimeter) : [35, 45, 55] } as LengthBoundSpec;
const FIT_BOUNDS = { (millimeter) : [0.1, 0.4, 1.2] } as LengthBoundSpec;
const COMBINER_BOUNDS = { (degree) : [20, 45, 70] } as AngleBoundSpec;

function makeBlock(context is Context, id is Id, x1, y1, z1, x2, y2, z2) returns Query
{
    fCuboid(context, id, {
        "corner1" : vector(x1, y1, z1) * millimeter,
        "corner2" : vector(x2, y2, z2) * millimeter
    });
    return qCreatedBy(id, EntityType.BODY);
}

function cylinderY(context is Context, id is Id, x, y1, y2, z, radius) returns Query
{
    fCylinder(context, id, {
        "bottomCenter" : vector(x, y1, z) * millimeter,
        "topCenter" : vector(x, y2, z) * millimeter,
        "radius" : radius * millimeter
    });
    return qCreatedBy(id, EntityType.BODY);
}

function cut(context is Context, id is Id, target is Query, tool is Query)
{
    opBoolean(context, id, {
        "tools" : tool,
        "targets" : target,
        "operationType" : BooleanOperationType.SUBTRACTION
    });
}

function label(context is Context, body is Query, name is string, shade is Color)
{
    setProperty(context, {
        "entities" : body,
        "propertyType" : PropertyType.NAME,
        "value" : name
    });
    setProperty(context, {
        "entities" : body,
        "propertyType" : PropertyType.APPEARANCE,
        "value" : shade
    });
}

annotation { "Feature Type Name" : "OpenFrame V0 layout",
             "Feature Type Description" : "Separate frame, optics, and electronics fit-check solids. Edit dimensions here before printing." }
export const openFrameV0 = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Bridge width" }
        isLength(definition.bridgeWidth, BRIDGE_BOUNDS);
        annotation { "Name" : "Temple length" }
        isLength(definition.templeLength, TEMPLE_BOUNDS);
        annotation { "Name" : "OLED to lens start" }
        isLength(definition.oledToLens, OPTICAL_BOUNDS);
        annotation { "Name" : "Combiner yaw" }
        isAngle(definition.combinerAngle, COMBINER_BOUNDS);
        annotation { "Name" : "Fit clearance" }
        isLength(definition.clearance, FIT_BOUNDS);
        annotation { "Name" : "Show purchased-part envelopes", "Default" : true }
        definition.showReferences is boolean;
    }
    {
        var bridge = definition.bridgeWidth / millimeter;
        var arm = definition.templeLength / millimeter;
        var oledGap = definition.oledToLens / millimeter;
        var fit = definition.clearance / millimeter;
        var ringX = (64 + bridge) / 2;
        var edgeX = ringX + 32;
        var black = color(0.10, 0.12, 0.15);
        var dark = color(0.20, 0.23, 0.27);
        var blue = color(0.13, 0.36, 0.76);
        var silver = color(0.68, 0.71, 0.76);

        // Front: two open eyeglass rims joined by a top bridge.
        var leftOuter = makeBlock(context, id + "leftRim", -ringX - 32, -3, -23, -ringX + 32, 3, 23);
        var rightOuter = makeBlock(context, id + "rightRim", ringX - 32, -3, -23, ringX + 32, 3, 23);
        var bridgeSolid = makeBlock(context, id + "bridge", -bridge / 2 - 2, -3, 13,
                              bridge / 2 + 2, 3, 21);
        opBoolean(context, id + "frontUnion", {
            "tools" : qUnion([leftOuter, rightOuter, bridgeSolid]),
            "operationType" : BooleanOperationType.UNION
        });
        var front = leftOuter;
        var leftOpening = makeBlock(context, id + "leftOpening", -ringX - 27, -4, -18,
                              -ringX + 27, 4, 18);
        cut(context, id + "leftRimCut", front, leftOpening);
        var rightOpening = makeBlock(context, id + "rightOpening", ringX - 27, -4, -18,
                               ringX + 27, 4, 18);
        cut(context, id + "rightRimCut", front, rightOpening);
        label(context, front, "PRINT - front frame fit check", black);

        // Temples are independent printable parts. Hinge/ear geometry is intentionally plain V0.
        var leftArm = makeBlock(context, id + "leftTemple", -edgeX - 3, 0, 11,
                          -edgeX + 4, arm, 19);
        label(context, leftArm, "PRINT - left temple", black);
        var rightArm = makeBlock(context, id + "rightTemple", edgeX - 4, 0, 11,
                           edgeX + 3, arm, 19);
        label(context, rightArm, "PRINT - right temple", black);

        // Right optical pod: open-front shell with removable OLED and lens pieces.
        var opticX = edgeX + 18;
        var pod = makeBlock(context, id + "podBlank", edgeX - 2, -32, -21,
                      edgeX + 38, 58, 21);
        var podCavity = makeBlock(context, id + "podCavity", edgeX + 1, -33, -18,
                            edgeX + 35, 55, 18);
        cut(context, id + "hollowPod", pod, podCavity);
        label(context, pod, "PRINT - removable right optical pod", black);

        var lensRing = cylinderY(context, id + "lensRingOuter", opticX, -8, -4, 0, 19);
        var lensHole = cylinderY(context, id + "lensRingHole", opticX, -9, -3, 0,
                                 17 + fit / 2);
        cut(context, id + "boreLensRing", lensRing, lensHole);
        label(context, lensRing, "PRINT - 34 mm lens retaining ring", dark);

        var oledY = -6 + oledGap;
        var oledSled = makeBlock(context, id + "oledSledBlank", opticX - 18, oledY - 1, -18,
                           opticX + 18, oledY + 6, 18);
        var oledSeat = makeBlock(context, id + "oledSeat", opticX - 14 - fit / 2,
                           oledY - 2, -15 - fit / 2, opticX + 14 + fit / 2,
                           oledY + 4, 15 + fit / 2);
        cut(context, id + "hollowOledSled", oledSled, oledSeat);
        label(context, oledSled, "PRINT - adjustable OLED sled", dark);

        // The small acrylic is a reference body inside a removable perimeter carrier.
        var combinerX = ringX + 8;
        var carrier = makeBlock(context, id + "combinerCarrierBlank", combinerX - 13, -40, -15,
                          combinerX + 13, -37, 15);
        var carrierWindow = makeBlock(context, id + "combinerCarrierWindow",
                                combinerX - 10.5, -41, -12.5,
                                combinerX + 10.5, -36, 12.5);
        cut(context, id + "openCarrier", carrier, carrierWindow);
        var combinerPlate = makeBlock(context, id + "combinerAcrylic", combinerX - 10,
                                -39, -12.5, combinerX + 10, -38, 12.5);
        var combinerAxis = line(vector(combinerX, -38.5, 0) * millimeter,
                                vector(0, 0, 1));
        opTransform(context, id + "angleCarrier", {
            "bodies" : carrier,
            "transform" : rotationAround(combinerAxis, definition.combinerAngle)
        });
        opTransform(context, id + "angleAcrylic", {
            "bodies" : combinerPlate,
            "transform" : rotationAround(combinerAxis, definition.combinerAngle)
        });
        label(context, carrier, "PRINT - removable combiner carrier", dark);
        label(context, combinerPlate, "REFERENCE - 20 x 25 mm acrylic combiner", silver);

        // Left tray is open above the 21 x 17.8 x 15 mm XIAO Sense stack.
        var xiaoTray = makeBlock(context, id + "xiaoTrayBlank", -edgeX - 13, 28, 4,
                           -edgeX + 13, 58, 24);
        var xiaoPocket = makeBlock(context, id + "xiaoPocket", -edgeX - 10.5 - fit,
                             31 - fit, 7, -edgeX + 10.5 + fit, 55 + fit, 25);
        cut(context, id + "openXiaoTray", xiaoTray, xiaoPocket);
        label(context, xiaoTray, "PRINT - XIAO ESP32S3 Sense tray", black);

        // Opposite temple carries the removable pouch cell and MiniBoost.
        var batteryTray = makeBlock(context, id + "batteryTrayBlank", edgeX - 15, 65, -1,
                              edgeX + 15, 107, 11);
        var batteryPocket = makeBlock(context, id + "batteryPocket", edgeX - 12.5 - fit,
                                68 - fit, 2, edgeX + 12.5 + fit, 104 + fit, 12);
        cut(context, id + "openBatteryTray", batteryTray, batteryPocket);
        label(context, batteryTray, "PRINT - removable 500 mAh battery tray", black);

        var boostTray = makeBlock(context, id + "boostTrayBlank", edgeX - 13, 109, -1,
                            edgeX + 13, 134, 10);
        var boostPocket = makeBlock(context, id + "boostPocket", edgeX - 9 - fit,
                              112 - fit, 2, edgeX + 9 + fit, 130 + fit, 11);
        cut(context, id + "openBoostTray", boostTray, boostPocket);
        label(context, boostTray, "PRINT - Adafruit MiniBoost tray", black);

        // A small two-button carrier leaves wiring/actuation detail for the next revision.
        var controls = makeBlock(context, id + "buttonCarrier", -edgeX - 9, 64, 18,
                           -edgeX + 9, 83, 21);
        label(context, controls, "PRINT - two-button carrier", dark);

        if (definition.showReferences)
        {
            var xiao = makeBlock(context, id + "xiaoRef", -edgeX - 8.9, 33, 8,
                           -edgeX + 8.9, 54, 23);
            label(context, xiao, "REFERENCE - Seeed XIAO ESP32S3 Sense", blue);
            var battery = makeBlock(context, id + "batteryRef", edgeX - 12.5, 69, 2,
                              edgeX + 12.5, 104, 7);
            label(context, battery, "REFERENCE - 35 x 25 x 5 mm LiPo", silver);
            var boost = makeBlock(context, id + "boostRef", edgeX - 8.9, 114, 2,
                            edgeX + 8.9, 125.3, 7.6);
            label(context, boost, "REFERENCE - 17.8 x 11.3 x 5.6 MiniBoost", blue);
            var oled = makeBlock(context, id + "oledRef", opticX - 13.5, oledY,
                           -14, opticX + 13.5, oledY + 4, 14);
            label(context, oled, "REFERENCE - 0.96 inch OLED module envelope", blue);
            var lens = cylinderY(context, id + "lensRef", opticX, -7, -5, 0, 17);
            label(context, lens, "REFERENCE - 34 mm optical lens", silver);
            var button1 = makeBlock(context, id + "buttonOne", -edgeX - 7, 66, 21,
                              -edgeX - 1, 72, 27);
            label(context, button1, "REFERENCE - next button 6 mm", silver);
            var button2 = makeBlock(context, id + "buttonTwo", -edgeX + 1, 75, 21,
                              -edgeX + 7, 81, 27);
            label(context, button2, "REFERENCE - dismiss button 6 mm", silver);
        }
    });
