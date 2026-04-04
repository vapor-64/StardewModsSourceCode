<lane orientation="vertical"
      horizontal-content-alignment="middle">

    <!-- Title banner -->
    <banner background={@Mods/StardewUI/Sprites/BannerBackground}
            background-border-thickness="48,0"
            padding="12"
            text="Rest until..." />

    <!-- Main card -->
    <frame layout="content content"
           background={@Mods/StardewUI/Sprites/MenuBackground}
           border={@Mods/StardewUI/Sprites/MenuBorder}
           border-thickness="36,36,40,36"
           margin="0,12,0,0"
           padding="32,16">

        <lane orientation="vertical"
              horizontal-content-alignment="middle">

            <!-- Spinner row: HOUR  :  MINUTE  AM/PM -->
            <lane orientation="horizontal"
                  vertical-content-alignment="middle"
                  margin="0,8,0,16">

                <!-- Hour spinner -->
                <lane orientation="vertical"
                      horizontal-content-alignment="middle">
                    <image layout="28px 32px"
                           sprite={@Mods/vapor64.SitToPassTime/Sprites/Cursors:Plus}
                           focusable="true"
                           click=|HourUp()| />
                    <label font="dialogue"
                           text={HourDisplay}
                           margin="0,8" />
                    <image layout="28px 32px"
                           sprite={@Mods/vapor64.SitToPassTime/Sprites/Cursors:Minus}
                           focusable="true"
                           click=|HourDown()| />
                </lane>

                <!-- Colon separator -->
                <label font="dialogue"
                       text=":"
                       margin="16,0" />

                <!-- Minute spinner -->
                <lane orientation="vertical"
                      horizontal-content-alignment="middle">
                    <image layout="28px 32px"
                           sprite={@Mods/vapor64.SitToPassTime/Sprites/Cursors:Plus}
                           focusable="true"
                           click=|MinuteUp()| />
                    <label font="dialogue"
                           text={MinuteDisplay}
                           margin="0,8" />
                    <image layout="28px 32px"
                           sprite={@Mods/vapor64.SitToPassTime/Sprites/Cursors:Minus}
                           focusable="true"
                           click=|MinuteDown()| />
                </lane>

                <!-- AM/PM toggle -->
                <button text={AmPmLabel}
                        margin="24,0,0,0"
                        click=|ToggleAmPm()| />

            </lane>

            <!-- Warning (shown when selected time is already past) -->
            <label *if={ShowWarning}
                   text="Already past that time!"
                   color="#cc4444"
                   margin="0,0,0,8" />

            <!-- Confirm / Cancel -->
            <lane orientation="horizontal"
                  horizontal-content-alignment="middle">
                <button text="Confirm"
                        click=|Confirm()|
                        tooltip="Skip time to the selected hour" />
                <button text="Cancel"
                        click=|Cancel()|
                        margin="16,0,0,0"
                        tooltip="Stay awake" />
            </lane>

        </lane>
    </frame>
</lane>
