import { ImgTouchButton, TouchButton } from "@microsoft/msfs-garminsdk";
import { DisplayComponent, FSComponent, Subject } from "@microsoft/msfs-sdk";
import AcarsTabView from "./AcarsTabView";
import { GtcViewLifecyclePolicy } from "@microsoft/msfs-wtg3000-gtc";
import { loadFuelAndBalance } from "./WeightAndBalance.mjs";
import getAircraftIcao from "./AircraftModels.mjs";

// Data Link button that replaces the Music button on MFD Home
class DataLinkButton extends DisplayComponent {
  render() {
    return (
      <ImgTouchButton
        label={"ATC\nData Link"}
        imgSrc={"coui://html_ui/garmin-3000-acars/assets/anntena.png"}
        class={"gtc-directory-button"}
        onPressed={() => {
          this.props.service.changePageTo("CPDLC");
        }}
      />
    );
  }
}

export const onMfdHomePage = (ctor, props, service) => {
  if (!window.wtg3000gtc.GtcViewKeys.TextDialog)
    window.wtg3000gtc.GtcViewKeys.TextDialog = "KeyboardDialog";
  
  // Return the Data Link button instead of the Music button
  return new DataLinkButton({ service });
};

class WeightProxy extends DisplayComponent {
  constructor(props) {
    super(props);
    this.textState = Subject.create("LOAD UPLNK");
    this.canRequest = Subject.create(true);
  }
  destroy() {
    if (this.tm) {
      clearTimeout(this.tm);
      this.tm = null;
    }
  }
  render() {
    const isSf50 = getAircraftIcao() === "SF50";
    if(isSf50){
        return null;
    }
    return (
      <div class="wf-row" style={{ display:  "flex" }}>
        <TouchButton
          label={this.textState}
          isEnabled={this.canRequest}
          class={"wf-row wf-bottom-center-button"}
          onPressed={() => {
            if (!this.canRequest.get()) return;
            if (this.tm) {
              clearTimeout(this.tm);
              this.tm = null;
            }
            this.textState.set("UPLNK\nLoading");
            this.canRequest.set(true);
            loadFuelAndBalance(this.props.service, this.props.instance).then((res) => {
              this.textState.set(res ? "UPLNK LOADED" : "LOAD FAILED");
              this.tm = setTimeout(() => {
                this.textState.set("LOAD UPLNK");
                this.tm = null;
              }, 10000);
              this.canRequest.set(true);
            });
          }}
        />
        {this.props.originalRenderered}
      </div>
    );
  }
}
export const onWeightPage = (ctor, props, service, instance) => {
  const rendered = new ctor(props).render();
  return new WeightProxy({
    originalRenderered: rendered,
    service,
    instance
  });
};

export const onMfdHomePageLiv2AirCj3 = (ctor, props, service) => {
  if (!window.wtg3000gtc.GtcViewKeys.TextDialog)
    window.wtg3000gtc.GtcViewKeys.TextDialog = "KeyboardDialog";
  
  // Return the Data Link button instead of the Music button
  return new DataLinkButton({ service });
};
export const registerViews = (ctx, fms) => {
  ctx.registerView(
    GtcViewLifecyclePolicy.Persistent,
    "CPDLC",
    "MFD",
    (gtcService, controlMode, displayPaneIndex) => {
      return (
        <AcarsTabView
          gtcService={gtcService}
          displayPaneIndex={displayPaneIndex}
          controlMode={controlMode}
          fms={fms}
        />
      );
    },
  );
};
