(function initializeRouteMap() {
    const routeData = document.getElementById("map-data");
    const mapContainer = document.getElementById("route-map");
    const mapStatus = document.getElementById("map-status");

    if (!routeData || !mapContainer || !mapStatus) {
        return;
    }

    const routes = JSON.parse(routeData.textContent);
    const routeColors = ["#557fdb", "#25a793", "#d7972f", "#bf66c9"];
    const routeButtons = document.querySelectorAll(".map-route");
    const svgNamespace = "http://www.w3.org/2000/svg";

    let map = null;
    let routeLayers = null;
    let selectedRouteId = null;

    function getRouteColor(route) {
        return routeColors[(route.id - 1) % routeColors.length];
    }

    function createPopupContent(text) {
        // Leaflet accepts HTML; textContent keeps labels safe if mock data is replaced.
        const content = document.createElement("span");
        content.textContent = text;
        return content;
    }

    function initializeStreetMap() {
        if (!window.L) {
            mapStatus.textContent =
                "Offline route overview: the street map could not load. " +
                "Sample connections are shown below; these are not driving directions.";
            return;
        }

        document.getElementById("map-loading")?.remove();
        map = L.map(mapContainer, { scrollWheelZoom: false });
        routeLayers = L.layerGroup().addTo(map);

        const tileLayer = L.tileLayer("https://tile.openstreetmap.org/{z}/{x}/{y}.png", {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        });

        tileLayer.on("tileerror", function showTileError() {
            mapStatus.textContent =
                "Map tiles are unavailable. Route lines and stops still show sample coordinates. " +
                "These are not driving directions.";
        });

        tileLayer.addTo(map);
    }

    function drawStreetRoutes(visibleRoutes) {
        routeLayers.clearLayers();

        visibleRoutes.forEach(function drawRoute(route) {
            const color = getRouteColor(route);
            const routeLine = L.polyline(route.coordinates, {
                color: color,
                weight: 5,
                opacity: 0.85
            });

            routeLine.addTo(routeLayers);
            routeLine.bindPopup(createPopupContent(route.name));
            routeLine.on("click", function selectMapRoute() {
                showRoutes(route.id);
            });

            route.coordinates.forEach(function drawPickupPoint(coordinate, index) {
                const marker = L.circleMarker(coordinate, {
                    color: color,
                    fillColor: color,
                    fillOpacity: 1,
                    radius: 7,
                    weight: 2
                });

                marker.addTo(routeLayers);
                marker.bindPopup(createPopupContent(route.stops[index]));
            });
        });

        const coordinates = visibleRoutes.flatMap(route => route.coordinates);
        map.fitBounds(coordinates, { padding: [35, 35], maxZoom: 13 });
    }

    function createSvgElement(tagName, attributes) {
        const element = document.createElementNS(svgNamespace, tagName);

        Object.entries(attributes).forEach(function applyAttribute([name, value]) {
            element.setAttribute(name, String(value));
        });

        return element;
    }

    function createCoordinateProjector() {
        const coordinates = routes.flatMap(route => route.coordinates);
        const latitudes = coordinates.map(coordinate => coordinate[0]);
        const longitudes = coordinates.map(coordinate => coordinate[1]);

        const minimumLatitude = Math.min(...latitudes);
        const maximumLatitude = Math.max(...latitudes);
        const minimumLongitude = Math.min(...longitudes);
        const maximumLongitude = Math.max(...longitudes);

        // Reserve space around the drawing for pickup labels.
        return function projectCoordinate([latitude, longitude]) {
            const x = 50 + (longitude - minimumLongitude) /
                Math.max(maximumLongitude - minimumLongitude, 0.001) * 420;
            const y = 440 - (latitude - minimumLatitude) /
                Math.max(maximumLatitude - minimumLatitude, 0.001) * 380;

            return [x, y];
        };
    }

    function drawOfflineRoutes(visibleRoutes) {
        const drawing = createSvgElement("svg", {
            viewBox: "0 0 700 500",
            role: "img",
            "aria-label": "Schematic route overview with sample pickup locations"
        });
        const projectCoordinate = createCoordinateProjector();

        visibleRoutes.forEach(function drawRoute(route) {
            const color = getRouteColor(route);
            const points = route.coordinates
                .map(coordinate => projectCoordinate(coordinate).join(","))
                .join(" ");

            const routeLine = createSvgElement("polyline", {
                points: points,
                fill: "none",
                stroke: color,
                "stroke-width": 4
            });

            drawing.append(routeLine);

            route.coordinates.forEach(function drawPickupPoint(coordinate, index) {
                const [x, y] = projectCoordinate(coordinate);
                const marker = createSvgElement("circle", {
                    cx: x,
                    cy: y,
                    r: 6,
                    fill: color
                });
                const label = createSvgElement("text", {
                    x: x + 10,
                    y: y - 10,
                    fill: "currentColor",
                    "font-size": 12
                });

                label.textContent = route.stops[index];
                drawing.append(marker, label);
            });
        });

        mapContainer.replaceChildren(drawing);
    }

    function updateRouteDetails() {
        const selectedRoute = routes.find(route => route.id === selectedRouteId);
        const routeName = document.getElementById("map-route-name");
        const routeDescription = document.getElementById("map-route-description");
        const pickupList = document.getElementById("map-stops");

        routeName.textContent = selectedRoute ? selectedRoute.name : "All routes";
        routeDescription.textContent = selectedRoute
            ? selectedRoute.distanceKm + " km · " + selectedRoute.stops.length + " sample stops"
            : "Select a route to see its pickup points.";

        pickupList.replaceChildren();

        selectedRoute?.stops.forEach(function addPickupLabel(stop) {
            const item = document.createElement("li");
            item.textContent = stop;
            pickupList.append(item);
        });

        routeButtons.forEach(function updateRouteButton(button) {
            const selected = Number(button.dataset.routeId) === selectedRouteId;
            button.classList.toggle("selected", selected);
            button.setAttribute("aria-pressed", String(selected));
        });
    }

    function showRoutes(routeId) {
        selectedRouteId = routeId;

        const visibleRoutes = routeId === null
            ? routes
            : routes.filter(route => route.id === routeId);

        if (map) {
            drawStreetRoutes(visibleRoutes);
        } else {
            drawOfflineRoutes(visibleRoutes);
        }

        updateRouteDetails();
    }

    routeButtons.forEach(function bindRouteButton(button) {
        button.addEventListener("click", function selectRoute() {
            showRoutes(Number(button.dataset.routeId));
        });
    });

    document.getElementById("show-all-routes").addEventListener("click", function showAllRoutes() {
        showRoutes(null);
    });

    document.getElementById("fit-map").addEventListener("click", function fitVisibleRoutes() {
        showRoutes(selectedRouteId);
    });

    initializeStreetMap();
    showRoutes(null);
})();
