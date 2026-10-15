# OpenFrame monocular prototype BOM

This bill of materials covers the first low-cost, monocular OpenFrame hardware
prototype. It is intended to prove the notification UI, display optics, comfort,
and frame geometry before investing in a custom PCB or higher-resolution
microdisplay.

The original parts prices were checked against the linked US retailer listings on
August 3, 2026. The eyeglass-lens price was checked on October 15, 2026.
Prices may change. The ESP32 development board and 3D printer are assumed to be
available already. The selected listings prioritize the lowest practical
purchase quantity rather than the lowest per-unit price.

## Complete wearable prototype — Amazon-only purchasing list

| Component | Final specification | Used in one wearable | Purchase quantity | Checked cost | Purchase link |
| --- | --- | ---: | ---: | ---: | --- |
| Monochrome OLED | **White**, 0.96 inch, 128 x 64, SSD1306, I2C, four-pin, 3.3 V compatible | 1 | 1 display | $6.99 | [HiLetgo single display](https://www.amazon.com/dp/B06XRBTBTB) |
| Collimating lens | Optical-glass biconvex lens, 34 mm diameter and 45 mm focal length | 1 | 1 set | $7.99 | [Pop-Tech lens set](https://www.amazon.com/dp/B012HYF4A2) |
| Eyeglass lenses | LenzReborn precut, non-polarized, transitional replacement lenses for Ray-Ban Meta Wayfarer RW4008 / Gen 2 RW4012, 53 mm; listed thickness 1.2 mm. Pair selected by the builder. | 2 | 1 pair | $7.50 | [LenzReborn lens pair](https://www.amazon.com/dp/B0GDXVHFDR) |
| Optical combiner | Two-way acrylic mirror, 30% light transmission, 12 x 12 x 0.04 inches | 1 cut piece | 3 sheets | $10.27 | [Three-sheet acrylic two-way mirror pack](https://www.amazon.com/dp/B0FPMC4M2F) |
| Notification controls | 6 x 6 x 6 mm, four-pin momentary tactile pushbuttons | 2 | 10 switches | $5.29 | [Corpco ten-pack](https://www.amazon.com/dp/B01GN79QF8) |
| Hookup wire | Flexible, stranded, tinned-copper 30 AWG silicone wire, six colors, 10 feet each | As needed | 1 six-color set | $9.29 | [Fermerry wire set](https://www.amazon.com/dp/B089D31ZX6) |
| Battery | Protected 3.7 V, 500 mAh LiPo with JST-PH 2.0 connector; 35 x 25 x 5 mm | 1 | 1 battery | $7.59 | [Liter 500 mAh battery](https://www.amazon.com/dp/B09FLYGKK3) |
| 5 V boost board | Adafruit MiniBoost 5 V, 1 A, TPS61023, with true-disconnect enable pin | 1 | 1 board | $6.45 | [Adafruit MiniBoost](https://www.amazon.com/dp/B0979MJJSK) |
| Battery socket lead | JST-PH 2.0 two-pin male/female silicone pigtail pair; use the half that mates with the battery | 1 mating lead | 5 pairs | $6.49 | [SIM&NAT five-pair cable pack](https://www.amazon.com/dp/B08SC3F658) |
| Off-device battery charger | Adafruit Micro Lipo USB charger ADA1304, default 100 mA charge rate | 1 | 1 board | $13.95 | [Adafruit Micro Lipo USB charger](https://www.amazon.com/dp/B00OKJFEQ2) |
| Power switch | Miniature SS12F15 SPDT slide switch | 1 | 20 switches | $5.99 | [Mini-switch 20-pack](https://www.amazon.com/dp/B0DN69L9SG) |
| Electrical insulation | Black 1/8-inch, 3:1 adhesive-lined heat-shrink tubing | Less than 1 foot | 1 roll | $5.88 | [Heat-shrink roll](https://www.amazon.com/dp/B0CZMZKR69) |

**Parts subtotal: $93.68.** Tax and shipping are not included. The
display-and-optics bench subset is $39.83; the remaining $53.85 adds the
eyeglass lenses, protected battery power, off-device charging, a physical power
switch, the battery socket lead, and insulated final wiring. Every purchase link in this table is an Amazon
listing. Prices and availability can change.

The eyeglass lenses are separate from the 34 mm optical lens in the display pod.
The 53 mm size does not establish their exact perimeter. The current OpenFrame
CAD has no lens-retaining groove; measure the delivered pair and revise the
frame front before treating them as a snap-fit part. Transitional lenses may
darken outdoors.

## Items intentionally omitted or deferred

- **M2 heat-set insert assortment:** use snap-fits or M2 screws in undersized
  printed holes for the first frame. Add inserts after the geometry stabilizes
  and repeated disassembly becomes useful.
- **Optical flocking tape:** begin with a matte-black printed display shroud or
  existing black tape. Buy flocking only if bench testing shows objectionable
  internal reflections.
- **Large button and switch assortments:** the small listings above already
  provide sufficient build spares.
- **Battery hardware during optical testing:** power the bench prototype over
  USB and defer all wearable power purchases until the display path works.

The MiniBoost and off-device charger replace the previously listed PowerBoost
1000C. That board's charger can supply up to 1 A, which is too aggressive for
this 500 mAh prototype cell. The selected charger remains at its default 100 mA
rate. If the exact ESP32 board includes a compatible LiPo connector, charger
limited to 500 mA or less, and power regulation, omit both separate boards only
after verifying the board documentation and battery polarity.

## Final display choice

The prototype uses the **white** SSD1306 OLED. White provides clearer small text
and icons, better visibility against varied backgrounds, and more predictable
reflection from an inexpensive neutral beamsplitter than blue. Do not purchase
the common yellow-and-blue version: its two colors occupy fixed physical regions
of the panel and cannot be assigned freely per pixel.

The display mount should remain removable so later revisions can test a color LCD
or high-resolution micro-OLED without redesigning the entire frame.

## Optional tools and consumables

- Temperature-controlled soldering iron and electronics solder
- Breadboard and jumper wires
- Digital multimeter
- USB power meter
- Black PETG filament for early frame iterations

## Electrical and optical safety

- Build and test the display, controls, and optics on USB power before adding the
  battery.
- Confirm the ESP32 board's input-voltage requirements. Use the MiniBoost's
  regulated 5 V output for a typical USB-powered development board; do not feed
  5 V into a 3.3 V-only pin.
- Verify JST red/black polarity at both connectors with a multimeter before
  connecting the LiPo. Connector housings from different vendors may use opposite
  polarity.
- Remove the battery from the wearable and disconnect it from the boost board
  before charging it with the Micro Lipo. Leave its charge-rate jumper open so
  it uses the default 100 mA rate.
- Use only a protected LiPo cell. Do not wear, charge, or continue testing a cell
  that is hot, swollen, punctured, or otherwise damaged.
- Keep the beamsplitter's protective film installed during cutting and fitting.
  Round exposed corners and avoid unsupported glass near the eye.
- Never aim the optical path at the sun or another intense light source.

## Recommended build order

1. Connect the white OLED to the ESP32 and render a mirrored notification card.
2. Add the two buttons and verify next-notification and dismiss actions.
3. Bench-test the OLED, lens, and combiner while powered over USB.
4. Adjust lens distance and combiner angle until the image is sharp at a relaxed
   viewing distance.
5. CAD and print a removable display pod before designing the complete frame.
6. Add the boost board, protected battery, power switch, and strain relief.
7. Test off-device charging with the battery removed from the frame.
8. Measure power consumption and temperature before performing a short wear test.

Detailed instructions: [prototype assembly guide](prototype-assembly.md).

A custom PCB, IMU, camera, networking hardware, and additional Bluetooth module
are intentionally outside the first prototype BOM.
