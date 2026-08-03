# OpenFrame monocular prototype assembly guide

This guide builds the first OpenFrame wearable from the parts in
[prototype-bom.md](prototype-bom.md). Complete each phase and its checkpoint
before moving on. The first power-up, optical alignment, and battery test all
happen on a bench; do not put an untested assembly on your face.

## What the finished prototype proves

- A white 128 x 64 notification is readable through a monocular combiner.
- The two buttons can advance and dismiss notifications.
- The display, ESP32, battery, and power boards can fit in a wearable frame.
- The optical pod can be repositioned without reprinting the entire frame.

This is a development prototype, not protective eyewear or a certified consumer
device. Do not use it while driving, cycling, or walking near traffic.

## Before starting

Gather the BOM parts plus a multimeter, soldering iron, solder, wire cutters,
wire strippers, helping hands, heat-resistant work surface, safety glasses,
small files or 400-800 grit sandpaper, masking tape, ruler, and calipers. A
breadboard and jumper wires are strongly recommended for the first tests.

Record the exact model of the ESP32 board and label its `3V3`, `5V`/`VIN`,
`GND`, `SDA`, and `SCL` pins from its manufacturer documentation. Do not assume
that an unfamiliar board uses the same pinout as another ESP32 board.

## Phase 1: prove the display on USB power

1. Leave the LiPo battery and both power boards disconnected.
2. Place the ESP32 and OLED on the bench so no exposed conductor can touch.
3. Connect OLED `GND` to ESP32 `GND`.
4. Connect OLED `VCC` to ESP32 `3V3`, unless the exact OLED documentation
   explicitly requires a different supply.
5. Connect OLED `SDA` and `SCL` to the board's documented I2C pins.
6. Power the ESP32 only from its USB connector.
7. Run an I2C scanner. A common SSD1306 address is `0x3C`, but use the address
   actually detected.
8. Load a test pattern with a border, four lines, and all four corners. Check for
   missing rows or unstable pixels.
9. Load the mock-notification firmware. Keep the image normal for direct viewing
   for now.

Checkpoint: the OLED remains stable for ten minutes and the ESP32, cable, and
display stay cool to the touch.

## Phase 2: add the notification buttons

1. Choose two free ESP32 GPIO pins that are safe to use as digital inputs.
2. Connect one side of each momentary button to `GND`.
3. Connect the other side of the first button to the next-notification GPIO and
   the second button to the dismiss GPIO.
4. Configure both inputs with the ESP32's internal pull-ups. A press should read
   low.
5. Add software debouncing of approximately 30-50 ms.
6. Confirm the first button advances exactly once per press and the second clears
   the active notification.

Checkpoint: perform 20 presses on each button without a double-trigger or missed
action.

## Phase 3: make an adjustable optical bench

Do this before CAD work. Tape or clamp the parts to cardboard, foam board, or a
simple printed rail so every distance and angle can move.

1. Change the notification to a horizontally mirrored image. Its reflection
   should then read normally.
2. Place the 34 mm biconvex lens in front of the OLED.
3. Start with approximately 45 mm between the OLED emitting surface and the
   lens. Slide it several millimeters in either direction to find the sharpest
   relaxed view.
4. Place an uncut portion of the two-way mirror at roughly 45 degrees to the
   optical axis.
5. Aim the reflected image toward the right eye while keeping the direct view of
   the room unobstructed.
6. Move the lens, display, and combiner until the complete image is visible
   without forcing the eye to focus at a painfully close distance.
7. Record the successful OLED-to-lens distance, lens-to-combiner distance,
   combiner angle, and combiner location relative to the eye.

Checkpoint: the whole notification can be read for one minute without
squinting or eye strain. Stop for discomfort, headache, dizziness, or double
vision.

## Phase 4: cut the acrylic combiner

Yes, the 12 x 12 inch sheet must be cut. Do not begin with the final tiny piece.
First make an approximately 40 x 50 mm test coupon. After alignment is proven,
reduce a fresh piece toward a target near 20 x 25 mm.

1. Put on safety glasses and work away from the electronics and battery.
2. Leave the protective film on both faces and cover the cut line with masking
   tape.
3. Mark a 40 x 50 mm rectangle with a fine marker and straightedge.
4. Support the entire sheet on a flat sacrificial surface.
5. For this approximately 1 mm acrylic, repeatedly score a straight line with an
   acrylic scoring knife and metal straightedge, then snap it over a sharp table
   edge using slow, even pressure.
6. If scoring produces cracks, stop and use a fine-tooth saw with the sheet
   clamped between sacrificial boards. Do not use a laser unless the seller
   confirms the material and coating are laser-safe.
7. Round every corner and smooth every exposed edge. Reject any piece with a
   crack, sharp corner, deep scratch, or flaking reflective layer.
8. Remove only a small corner of protective film to determine which surface
   carries the reflective coating. Mount that surface toward the OLED when
   practical; a front-surface reflection usually reduces the double image.
9. Re-test the 40 x 50 mm coupon on the optical bench.
10. Mark the smallest area that still shows the entire notification during
    normal eye movement. Cut a fresh final piece, beginning near 20 x 25 mm,
    then round and polish its edges.

Checkpoint: the final piece shows the whole display, has no sharp edge, and is
retained without bending. Because this sheet transmits only 30% of room light,
keep the combiner small and outside the center of the normal sightline.

## Phase 5: CAD the modular frame

1. Measure every physical part with calipers; do not rely only on retailer
   dimensions.
2. Start with a conventional frame front, nose bridge, and two temples.
3. Make the right-side optical pod a separate removable part.
4. Add a sliding OLED/lens carriage with at least +/-5 mm of focus adjustment.
5. Give the combiner holder several positions around 45 degrees plus height and
   lateral adjustment.
6. Place the ESP32 on one temple and the battery/boost board on the other to
   balance weight.
7. Capture PCBs by their edges or screw holes. Do not press on components or
   solder joints.
8. Put the LiPo in a smooth removable pocket with clearance around every side.
   Nothing may screw into, clamp, fold, or sharply bend the pouch cell.
9. Add wire channels, strain relief, ventilation, button openings, and access to
   the battery connector.
10. Keep the combiner on a breakaway or removable mount so an impact does not
    drive it toward the eye.
11. Print the first fit-check frame without electronics and verify nose, ear,
    hinge, and optical-pod clearances.

Checkpoint: all parts install and remove without force, the battery cannot be
pinched, and the combiner cannot touch the eye at any adjustment position.

## Phase 6: build the wearable wiring harness

Keep the battery disconnected while soldering.

1. Cut wires only long enough to reach through the planned channels with a small
   service loop at hinges and connectors.
2. Solder and insulate the OLED and button connections tested in Phases 1-2.
3. Cut the JST-PH 2.0 pigtail half that mates with the battery to a useful
   length. Before
   soldering, plug it into the battery and use a multimeter to identify its
   actual positive and negative conductors; do not trust wire colors alone.
4. Solder pigtail positive to MiniBoost `IN` and pigtail negative to `GND`.
   Insulate both joints. The battery must remain unplugged while soldering.
5. Connect MiniBoost `5V` to the ESP32's documented `5V`/`VIN` input and connect
   grounds. Never connect 5 V to a `3V3` pin.
6. Use two slide-switch contacts between MiniBoost `EN` and `GND`. When those
   contacts close, the booster turns off. Check the contacts with a continuity
   meter before soldering and isolate the unused contact.
7. Cover every joint with heat-shrink and inspect for stray wire strands.
8. With no battery connected, check resistance between `5V` and `GND`. Stop and
   find the short if the meter indicates near-zero resistance.
9. Verify polarity at the battery and socket lead again before plugging in the
   protected battery.
10. Turn on the boost board and measure its output before connecting the ESP32.
11. Turn it off, connect the ESP32, then power the complete assembly on the bench.

Checkpoint: notifications and both buttons operate from battery power for 15
minutes, voltage remains stable, and no component or wire becomes hot.

## Phase 7: charge the battery safely

The Micro Lipo is an off-device charger. Do not charge the battery while it is
inside the glasses or connected to the MiniBoost.

1. Turn the glasses off.
2. Unplug the battery by gripping the JST housing, never the wires.
3. Inspect the cell for swelling, dents, punctures, damaged insulation, loose
   wires, or heat. Retire a damaged cell; do not charge it.
4. Confirm the Micro Lipo charge-rate jumper remains open. Its default 100 mA
   rate is below the battery's 500 mA maximum.
5. Put the charger and battery on a nonflammable surface away from the frame.
6. Connect the battery to the Micro Lipo, verify connector polarity, then plug
   its integrated USB-A connector into a USB power source.
7. Remain present while charging. Disconnect USB for heat, swelling, unusual
   odor, or unexpected wiring behavior.
8. When charging completes, unplug USB first, then disconnect and reinstall the
   battery in the powered-off glasses.

Amazon purchase references: [Liter 500 mAh battery](https://www.amazon.com/dp/B09FLYGKK3),
[Adafruit MiniBoost](https://www.amazon.com/dp/B0979MJJSK), and
[Adafruit Micro Lipo USB charger](https://www.amazon.com/dp/B00OKJFEQ2). The
[JST-PH 2.0 pigtail pack](https://www.amazon.com/dp/B08SC3F658) supplies the
mating battery lead for the MiniBoost.

## Phase 8: final integration and wear test

1. Install the electronics with the battery disconnected.
2. Verify no wire crosses a sharp printed edge or stretches when a temple opens.
3. Secure the lens and combiner mechanically. Do not rely on exposed hot glue
   near the eye.
4. Clean the optics with a lens cloth; do not use solvents on the acrylic.
5. Connect the battery and run the closed frame on the bench for 30 minutes.
6. Check temperatures at the ESP32, boost board, battery, and skin-facing areas.
7. Perform the first wear test while seated, with another person nearby, for no
   more than five minutes.
8. Check vision, comfort, pressure points, balance, skin temperature, and cable
   strain. Stop for discomfort, eye fatigue, headache, dizziness, heat, odor, or
   unstable electronics.
9. Increase wear time gradually only after inspecting the frame and battery
   between tests.

## Acceptance checklist

- [ ] OLED test pattern and notifications render without missing pixels.
- [ ] Next and dismiss buttons work reliably.
- [ ] Reflected text reads normally rather than mirrored.
- [ ] Complete notification is visible with relaxed focus.
- [ ] Combiner is small, smooth, crack-free, and outside the central sightline.
- [ ] Optical pod cannot contact the eye.
- [ ] All conductors are insulated and strain-relieved.
- [ ] JST polarity and boost output were measured with a multimeter.
- [ ] Battery is removable and never charged while worn or enclosed.
- [ ] Thirty-minute bench test completes without resets or abnormal heat.
- [ ] Five-minute seated wear test completes without visual discomfort.

## Common problems

| Symptom | First checks |
| --- | --- |
| OLED is blank | Confirm 3.3 V, common ground, detected I2C address, SDA/SCL assignment, and reset requirements. |
| Text is backward | Mirror the framebuffer horizontally in firmware. |
| Image is doubled | Put the reflective coating toward the OLED, add a matte-black shroud, and check combiner flatness. |
| Only part of the display is visible | Use a larger combiner temporarily, move it closer to the eye, or adjust angle and lateral position. |
| Image will not focus | Slide the OLED around the 45 mm starting distance; check lens centering. |
| ESP32 resets on battery | Check boost voltage under load, wiring resistance, battery charge, and the board's documented power input. |
| Combiner view is too dark | Make it smaller and move it outside the central sightline; a later revision can use a higher-transmission combiner. |
