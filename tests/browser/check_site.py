"""
Browser checks for the published site: the welcome tour, themes, a few full plays,
and a WCAG AA colour-contrast audit of every screen in light and dark, on desktop and phone.

Run against a local copy served at SITE_URL (see .github/workflows/deploy.yml, or the README):
    python tests/browser/check_site.py
Exits non-zero if anything fails.
"""
import os
import sys

from axe_playwright_python.sync_playwright import Axe
from playwright.sync_api import sync_playwright

URL = os.environ.get("SITE_URL", "http://localhost:8080/what-the-code/")
LEVELS_1_TO_11 = '{"1":3,"2":3,"3":3,"4":3,"5":3,"6":3,"7":3,"8":3,"9":3,"10":3,"11":3}'
LEVELS_1_TO_6 = '{"1":3,"2":3,"3":3,"4":3,"5":3,"6":3}'

failures = []
passed = 0


def check(name, ok, detail=""):
    global passed
    if ok:
        passed += 1
        print(f"  PASS  {name}")
    else:
        failures.append(f"{name} {detail}".strip())
        print(f"  FAIL  {name} {detail}")


def boot(page, url=URL):
    page.goto(url)
    page.wait_for_selector(".board", timeout=60000)


def step_until(page, selector, limit=40):
    """Presses Step until something appears. Returns whether it did."""
    for _ in range(limit):
        if page.query_selector(selector):
            return True
        page.click(".run-controls >> text=Step")
        page.wait_for_timeout(200)
    return page.query_selector(selector) is not None


# ---------- Welcome tour, settings and themes ----------

def tour_and_theme(browser, devices):
    print("Welcome tour and themes")
    ctx = browser.new_context(viewport={"width": 1280, "height": 860}, color_scheme="light")
    page = ctx.new_page()
    boot(page)
    page.wait_for_selector(".tour-card", state="visible", timeout=10000)
    check("tour shows on a first visit", page.is_visible(".tour-card"))
    check("light device gets the light theme", page.get_attribute("html", "data-theme") == "light")
    page.wait_for_timeout(300)
    check("keyboard focus starts on Next", "tour-next" in page.evaluate("document.activeElement.className"))
    page.click(".tour-next")
    page.click(".tour-next")
    check("last step says Let's go", page.inner_text(".tour-next").strip() == "Let's go")
    page.click(".tour-next")
    page.wait_for_timeout(200)
    check("tour closes when finished", page.query_selector(".tour") is None)
    check("finishing is remembered", page.evaluate("localStorage.getItem('whatthecode.welcomed')") == "1")
    boot(page)
    page.wait_for_timeout(800)
    check("no tour on a return visit", page.query_selector(".tour") is None)

    page.click(".settings-btn")
    page.click("text=How to play")
    page.wait_for_selector(".tour-card", state="visible")
    check("tour replays from Settings", page.is_visible(".tour-card"))
    page.keyboard.press("Escape")
    page.wait_for_timeout(200)
    check("Escape closes the tour", page.query_selector(".tour") is None)

    page.click(".settings-btn")
    page.click(".theme-choice >> text=Dark")
    page.wait_for_timeout(150)
    check("choosing Dark applies it", page.get_attribute("html", "data-theme") == "dark")
    boot(page)
    page.wait_for_timeout(300)
    check("Dark is remembered", page.get_attribute("html", "data-theme") == "dark")
    check("browser bar colour follows the theme", page.get_attribute('meta[name="theme-color"]', "content") == "#0F1726")
    page.click(".settings-btn")
    page.click(".theme-choice >> text=Device")
    page.wait_for_timeout(150)
    check("Device goes back to the device setting", page.get_attribute("html", "data-theme") == "light")
    page.emulate_media(color_scheme="dark")
    page.wait_for_timeout(200)
    check("Device follows a live switch to dark", page.get_attribute("html", "data-theme") == "dark")
    ctx.close()

    # Players from before the tour existed shouldn't get it.
    ctx = browser.new_context(viewport={"width": 1280, "height": 860})
    ctx.add_init_script(f"if (!sessionStorage.seeded) {{ localStorage.setItem('whatthecode.progress.v1', '{LEVELS_1_TO_6}'); sessionStorage.seeded = 1; }}")
    page = ctx.new_page()
    boot(page)
    page.wait_for_timeout(800)
    check("no tour for players who already have progress", page.query_selector(".tour") is None)
    check("those players are marked as welcomed", page.evaluate("localStorage.getItem('whatthecode.welcomed')") == "1")
    ctx.close()

    ctx = browser.new_context(**devices["iPhone 13"], color_scheme="dark")
    page = ctx.new_page()
    page.goto(URL)
    check("theme is set before the app loads (no flash)", page.get_attribute("html", "data-theme") == "dark")
    page.wait_for_selector(".tour-card", state="visible", timeout=60000)
    for step in range(1, 4):
        page.wait_for_timeout(350)
        box = page.locator(".tour-card").bounding_box()
        vw, vh = page.viewport_size["width"], page.viewport_size["height"]
        check(f"phone: tour card {step} fits on screen",
              box["x"] >= 0 and box["y"] >= 0 and box["x"] + box["width"] <= vw and box["y"] + box["height"] <= vh)
        page.click(".tour-next")
    page.click(".concept-toggle")
    check("concept card opens with Tell me more", page.is_visible(".concept-code"))
    check("concept toggle reports it is open", page.get_attribute(".concept-toggle", "aria-expanded") == "true")
    ctx.close()


# ---------- Playing ----------

def gameplay(browser):
    print("Playing")
    # Reduced motion turns off the pulsing hint on a new player's first block, which Playwright won't click mid-pulse.
    ctx = browser.new_context(viewport={"width": 1280, "height": 900}, reduced_motion="reduce")
    ctx.add_init_script("localStorage.setItem('whatthecode.welcomed', '1');")
    page = ctx.new_page()
    boot(page)

    for _ in range(5):
        page.click(".palette-block")
    page.click(".btn-speed")
    page.click(".btn-speed")  # fast
    page.click(".btn-run")
    page.wait_for_selector(".status-fail", timeout=15000)
    check("driving off the board is caught", page.is_visible(".status-fail"))
    check("the block that crashed is highlighted", page.query_selector(".block.is-failed") is not None)

    page.click("text=Clear")
    for _ in range(3):
        page.click(".palette-block")
    page.click(".btn-run")
    page.wait_for_selector(".status-success", timeout=15000)
    check("Level 1 can be solved with blocks", page.is_visible(".status-success"))
    check("a badge toast appears", page.query_selector(".toast") is not None)

    page.click(".badges-btn")
    page.wait_for_selector(".cabinet")
    page.wait_for_timeout(200)
    check("badge cabinet takes keyboard focus", "cabinet-close" in page.evaluate("document.activeElement.className"))
    page.keyboard.press("Escape")
    page.wait_for_timeout(200)
    check("Escape closes the badge cabinet", page.query_selector(".cabinet") is None)
    check("focus goes back to the Badges button", "badges-btn" in page.evaluate("document.activeElement.className"))

    page.click(".level-stop >> nth=0")
    page.click(".mode-toggle >> text=Code")
    page.fill("#code-editor", "bot.MoveForward();\nbot.MoveForward();\nbot.MoveForward();")
    page.click(".btn-run")
    page.wait_for_selector(".status-success", timeout=15000)
    check("Level 1 can be solved by typing C#", page.is_visible(".status-success"))

    page.click(".mode-toggle >> text=Blocks")
    page.click(".mode-toggle >> text=Code")
    shown = page.input_value("#code-editor")
    check("switching back to Code shows the program as C#", shown.count("bot.MoveForward();") == 3, repr(shown[:60]))

    page.fill("#code-editor", "bot.MoveFoward();")
    page.wait_for_timeout(200)
    check("a typo gets a friendly suggestion", "Did you mean MoveForward" in page.inner_text(".code-error"))
    ctx.close()


# ---------- Colour contrast ----------

axe = Axe()
contrast_problems = {}


def audit(page, label):
    page.wait_for_timeout(450)  # let animations settle
    result = axe.run(page, options={"runOnly": ["color-contrast"]})
    for violation in result.response["violations"]:
        for node in violation["nodes"]:
            data = node["any"][0]["data"] if node["any"] else {}
            key = (node["target"][0], data.get("fgColor"), data.get("bgColor"))
            if key not in contrast_problems:
                contrast_problems[key] = label
                print(f"  FAIL  [{label}] {node['target'][0]}: {data.get('fgColor')} on {data.get('bgColor')} "
                      f"is {data.get('contrastRatio')}, needs {data.get('expectedContrastRatio')}")


def contrast(browser, devices, theme, phone):
    tag = f"{theme}, {'phone' if phone else 'desktop'}"
    print(f"Contrast: {tag}")
    options = dict(devices["iPhone 13"]) if phone else {"viewport": {"width": 1280, "height": 900}}
    ctx = browser.new_context(**options, color_scheme=theme, reduced_motion="reduce")
    ctx.add_init_script("localStorage.setItem('whatthecode.welcomed', '1');"
                        f"if (!localStorage.getItem('whatthecode.progress.v1')) localStorage.setItem('whatthecode.progress.v1', '{LEVELS_1_TO_11}');")
    page = ctx.new_page()
    boot(page)

    page.click(".level-stop >> nth=0")
    page.click(".concept-toggle")
    audit(page, f"{tag}: level 1 with concept open")
    for _ in range(5):
        page.click(".palette-block")
    page.click(".btn-speed")
    page.click(".btn-speed")
    page.click(".btn-run")
    page.wait_for_selector(".status-fail", timeout=15000)
    audit(page, f"{tag}: crash")
    page.click("text=Clear")
    for _ in range(3):
        page.click(".palette-block")
    page.click(".btn-run")
    page.wait_for_selector(".status-success", timeout=15000)
    audit(page, f"{tag}: solved")

    page.click(".level-stop >> nth=6")
    page.click("text=Stuck? Show a hint")
    page.click("text=Show another hint")
    page.click("text=Show the answer")
    audit(page, f"{tag}: hints and answer warning")
    page.click(".help-box .btn-reveal")
    page.click("text=Put the answer in my program")
    audit(page, f"{tag}: answer loaded")
    page.click(".btn-speed")  # back to slow so steps are easy to catch
    check(f"{tag}: stepping shows an If answering yes", step_until(page, ".run-chip.yes"))
    audit(page, f"{tag}: If answered yes")
    check(f"{tag}: stepping shows an If answering no", step_until(page, ".run-chip.no"))
    audit(page, f"{tag}: If answered no")
    page.click(".btn-stop")

    page.click(".mode-toggle >> text=Code")
    audit(page, f"{tag}: code mode")
    check(f"{tag}: code mode shows an if answering no", step_until(page, ".line-note.no"))
    audit(page, f"{tag}: code mode, if answered no")
    page.click(".btn-stop")
    page.fill("#code-editor", "bot.Jump();")
    page.click(".mode-toggle >> text=Blocks")
    page.wait_for_selector(".code-error-note")
    audit(page, f"{tag}: code error, can't switch back")
    page.fill("#code-editor", "")
    page.click(".mode-toggle >> text=Blocks")

    page.click(".settings-btn")
    audit(page, f"{tag}: settings")
    page.keyboard.press("Escape")
    page.click(".badges-btn")
    audit(page, f"{tag}: badges")
    page.keyboard.press("Escape")
    page.click("a[href=make]")
    page.wait_for_selector(".paint-cell")
    audit(page, f"{tag}: level editor")
    ctx.close()


def main():
    with sync_playwright() as p:
        browser = p.chromium.launch()
        tour_and_theme(browser, p.devices)
        gameplay(browser)
        for theme in ("light", "dark"):
            for phone in (False, True):
                contrast(browser, p.devices, theme, phone)
        browser.close()

    for (target, fg, bg), label in contrast_problems.items():
        failures.append(f"contrast: {target} {fg} on {bg} ({label})")
    print(f"\n{passed} checks passed, {len(failures)} failed, {len(contrast_problems)} contrast problems.")
    for f in failures:
        print(f"  - {f}")
    sys.exit(1 if failures else 0)


if __name__ == "__main__":
    main()
