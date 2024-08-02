import React from "react";
function TrackDetail(props){
    let temp =props.width
        return (
                    <table  className="table table-striped" >
                        <tbody>
                        <tr>
                            <th scope="row" style={{display:'inline-block',width:`${temp}px`}}>ID</th>
                            <td>{props.id}</td>
                        </tr>

                        <tr>
                            <th scope="row">Type</th>
                            <td>{props.type}</td>
                        </tr>

                        <tr>
                            <th scope="row">Velocity</th>
                            <td>{props.velocity}</td>
                        </tr>
                        <tr>
                            <th scope="row">Unit</th>
                            <td>{props.device}</td>
                        </tr>
                        <tr >
                            <th scope="row">Action</th>
                            <td>  <button onClick={props.goToTrack} style={{display:'inline-block',width:`${temp}px`, left :"0px"}}>
                                Go to Flight
                            </button>
                            </td>
                        </tr>
                        </tbody>
                    </table>
)

}
export default TrackDetail;