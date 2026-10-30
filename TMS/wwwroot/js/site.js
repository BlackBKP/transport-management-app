(function initializeSite() {
    const navigationButton = document.querySelector(".menu-toggle");
    const sidebar = document.querySelector(".sidebar");

    function setNavigationOpen(open) {
        sidebar?.classList.toggle("open", open);
        navigationButton?.setAttribute("aria-expanded", String(open));
    }

    navigationButton?.addEventListener("click", function toggleNavigation() {
        setNavigationOpen(!sidebar?.classList.contains("open"));
    });

    document.addEventListener("keydown", function closeNavigationOnEscape(event) {
        if (event.key === "Escape") {
            setNavigationOpen(false);
        }
    });

    document.addEventListener("click", function closeNavigationOutside(event) {
        if (!sidebar?.contains(event.target) && !navigationButton?.contains(event.target)) {
            setNavigationOpen(false);
        }
    });

    document.querySelectorAll(".cancel-form").forEach(function bindCancellation(form) {
        form.addEventListener("submit", function confirmCancellation(event) {
            const confirmed = window.confirm("Cancel this booking and release the seat?");

            if (!confirmed) {
                event.preventDefault();
            }
        });
    });

    const routeSelect = document.getElementById("routeId");
    const pickupSelect = document.getElementById("pickupPoint");
    const routeData = document.getElementById("route-data");

    if (!routeSelect || !pickupSelect || !routeData) {
        return;
    }

    const routes = JSON.parse(routeData.textContent);

    function updatePickupPoints(restoreSelection) {
        const selectedRouteId = Number(routeSelect.value);
        const selectedRoute = routes.find(route => route.id === selectedRouteId);
        const placeholder = selectedRoute ? "Select a pickup point" : "Select a route first";

        pickupSelect.replaceChildren(new Option(placeholder, ""));

        selectedRoute?.stops.forEach(function addPickupPoint(stop) {
            pickupSelect.add(new Option(stop, stop));
        });

        if (restoreSelection) {
            pickupSelect.value = pickupSelect.dataset.selected || "";
        }
    }

    routeSelect.addEventListener("change", function changeRoute() {
        updatePickupPoints(false);
    });

    updatePickupPoints(true);
})();

