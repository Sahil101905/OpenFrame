# OpenFrame monocular prototype BOM

This bill of materials covers the first low-cost, monocular OpenFrame hardware
prototype. It is intended to prove the notification UI, display optics, comfort,
and frame geometry before investing in a custom PCB or higher-resolution
microdisplay.

Prices are approximate Amazon US ranges as of August 2026 and may change. The
ESP32 development board and 3D printer are assumed to be available already.

## Core components

| Component | Final specification | Quantity | Estimated cost | Purchase link |
| --- | --- | ---: | ---: | --- |
| Monochrome OLED | **White**, 0.96 inch, 128 x 64, SSD1306, I2C, four-pin, 3.3 V compatible | 1 | $8-$15 | [Amazon search](https://www.amazon.com/s?k=0.96+white+128x64+SSD1306+I2C+OLED) |
| Collimating lens | Biconvex lens, approximately 34 mm diameter and 45 mm focal length | 1 | $8-$15 | [Amazon search](https://www.amazon.com/s?k=google+cardboard+34mm+biconvex+lens+45mm+focal+length) |
| Optical combiner | 50/50 acrylic beamsplitter or teleprompter mirror, approximately 3 mm thick | 1 sheet | $15-$30 | [Amazon search](https://www.amazon.com/s?k=50%2F50+acrylic+beamsplitter+mirror+teleprompter) |
| Battery | Protected 3.7 V, 500 mAh LiPo with JST-PH 2.0 connector | 1 | $10-$16 | [Amazon search](https://www.amazon.com/s?k=Adafruit+1578+500mAh+LiPo+battery) |
| Power and charging board | Adafruit PowerBoost 1000 Charger, providing LiPo charging and regulated 5 V output | 1 | $20-$30 | [Amazon search](https://www.amazon.com/s?k=Adafruit+PowerBoost+1000+Charger) |
| Notification controls | 6 x 6 mm momentary tactile pushbuttons | 2 | $6-$10 per pack | [Amazon search](https://www.amazon.com/s?k=6x6mm+tactile+push+button+switch+assortment) |
| Power switch | Miniature SPDT slide switch | 1 | $6-$10 per pack | [Amazon search](https://www.amazon.com/s?k=mini+SPDT+slide+switch+through+hole) |
| Hookup wire | Flexible, stranded, 30 AWG silicone wire in multiple colors | 1 kit | $10-$15 | [Amazon search](https://www.amazon.com/s?k=30+AWG+silicone+wire+6+color) |
| Battery connectors | JST-PH 2.0 two-pin pigtails | 1 kit | $7-$10 | [Amazon search](https://www.amazon.com/s?k=JST+PH+2.0+2+pin+pigtail+connector) |
| Electrical insulation | Heat-shrink assortment including 1-4 mm sizes | 1 kit | $6-$10 | [Amazon search](https://www.amazon.com/s?k=small+heat+shrink+tubing+assortment) |
| Frame fasteners | M2 brass heat-set inserts and matching screws in several lengths | 1 kit | $12-$20 | [Amazon search](https://www.amazon.com/s?k=M2+heat+set+inserts+screws+3D+printing+kit) |
| Stray-light control | Matte black optical flocking tape | 1 roll | $7-$12 | [Amazon search](https://www.amazon.com/s?k=matte+black+flocking+tape+optical) |

The expected core BOM cost is approximately **$115-$190**, excluding tax,
shipping, the ESP32, tools, and 3D-printing equipment.

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
- Confirm the ESP32 board's input-voltage requirements. Use the PowerBoost's
  regulated 5 V output for a typical USB-powered development board; do not feed
  5 V into a 3.3 V-only pin.
- Verify JST red/black polarity at both connectors with a multimeter before
  connecting the LiPo. Connector housings from different vendors may use opposite
  polarity.
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
6. Add the protected battery, charger, power switch, and strain relief.
7. Measure power consumption and temperature before performing a short wear test.

A custom PCB, IMU, camera, networking hardware, and additional Bluetooth module
are intentionally outside the first prototype BOM.
