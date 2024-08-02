import React from "react";

function DropDownMenu(props) {
    return (
        <div className="btn-group dropup"
             style={{display: 'block', border: 'outset', position: 'fixed'}}
             data-target="#navbarNavDropdown">
            <button type="button" className="btn btn-secondary dropdown-toggle" id="dropdownMenuButton1"
                    data-bs-toggle="dropdown" aria-expanded="false">
                {props.menuType}
            </button>
            <ul className="dropdown-menu" aria-labelledby="dropdownMenuButton1">
                {props.menu}
            </ul>
        </div>
    );
}

export default DropDownMenu;
